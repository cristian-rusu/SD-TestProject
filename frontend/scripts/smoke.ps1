param([string]$BaseUrl = 'http://localhost:5180')
$ErrorActionPreference = 'Stop'

# Run only against a disposable demo database: this creates a product and two orders.
function Assert-Equal($Actual, $Expected, [string]$Label) {
    if ($Actual -ne $Expected) { throw "$Label expected $Expected, got $Actual" }
    Write-Output "PASS: $Label ($Actual)"
}
function Post-Json([string]$Path, $Body) {
    Invoke-RestMethod -Method Post -Uri "$BaseUrl$Path" -ContentType 'application/json' -Body ($Body | ConvertTo-Json)
}

$spaResponse = Invoke-WebRequest -UseBasicParsing "$BaseUrl/"
Assert-Equal $spaResponse.StatusCode 200 'Published SPA'
if ($spaResponse.Content -notmatch '/assets/[^" ]+\.js') { throw 'Missing production JavaScript asset' }
$asset = Invoke-WebRequest -UseBasicParsing "$BaseUrl$($Matches[0])"
Assert-Equal $asset.StatusCode 200 'Production JavaScript'
foreach ($path in @('/api/not-an-endpoint', '/orders/not-a-guid')) {
    try { Invoke-WebRequest -UseBasicParsing "$BaseUrl$path" | Out-Null; throw 'Expected HTTP 404' }
    catch [System.Net.WebException] { Assert-Equal ([int]$_.Exception.Response.StatusCode) 404 "Unknown API path $path" }
}

$created = Post-Json '/catalog/products/' @{ name = 'Frontend smoke product'; description = 'Disposable validation record'; price = 12.5 }
$productId = $created.productId
Invoke-RestMethod -Method Post "$BaseUrl/catalog/products/$productId/publish" | Out-Null
$product = Invoke-RestMethod "$BaseUrl/catalog/products/$productId"
Assert-Equal $product.status 'Published' 'Product publication'
Post-Json "/inventory/products/$productId/replenish" @{ quantity = 10 } | Out-Null
$first = Post-Json '/orders/' @{ productId = $productId; quantity = 4 }
$order = Invoke-RestMethod "$BaseUrl/orders/$($first.orderId)"
Assert-Equal $order.status 'Confirmed' 'Order for 4'
Assert-Equal $order.lines[0].acceptedPrice 12.5 'Accepted price from Ordering'
$stock = Invoke-RestMethod "$BaseUrl/inventory/products/$productId"
Assert-Equal $stock.availableQuantity 6 'Stock after confirmation'
$second = Post-Json '/orders/' @{ productId = $productId; quantity = 20 }
$rejected = Invoke-RestMethod "$BaseUrl/orders/$($second.orderId)"
Assert-Equal $rejected.status 'Rejected' 'Order for 20'
$stock = Invoke-RestMethod "$BaseUrl/inventory/products/$productId"
Assert-Equal $stock.availableQuantity 6 'Stock after rejection'
Invoke-RestMethod -Method Post "$BaseUrl/catalog/products/$productId/discontinue" | Out-Null
$product = Invoke-RestMethod "$BaseUrl/catalog/products/$productId"
Assert-Equal $product.status 'Discontinued' 'Product discontinuation'
