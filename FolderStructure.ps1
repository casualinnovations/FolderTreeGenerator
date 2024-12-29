param (
    [Alias("md")][switch]$IncludeMd,
    [Alias("cs")][switch]$IncludeCs,
    [switch]$All
)

# Function to generate the tree structure
function Get-FolderTree {
    param (
        [string]$Path,
        [string]$Prefix = "",
        [bool]$IncludeCs,
        [bool]$IncludeMd,
        [bool]$AllFiles
    )

    # Get the list of directories, excluding bin and obj directories
    $directories = Get-ChildItem -Path $Path -Directory | Where-Object { $_.Name -notin @('bin', 'obj', 'node_modules') }
    $nodeModules = Get-ChildItem -Path $Path -Directory | Where-Object { $_.Name -eq 'node_modules' }

    # Create a file filter based on the parameters
    $fileFilters = @('*.csproj', '*.sln')
    if ($IncludeMd) {
        $fileFilters += '*.md'
    }
    if ($IncludeCs) {
        $fileFilters += '*.cs'
    }
    
    # Get the list of files based on the file filter
    $files = @()
    if ($AllFiles) {
        $files = Get-ChildItem -Path $Path -File
    } else {
        foreach ($filter in $fileFilters) {
            $files += Get-ChildItem -Path $Path -Filter $filter -File
        }
    }

    # Iterate through each file
    for ($j = 0; $j -lt $files.Count; $j++) {
        $file = $files[$j]
        $isLastFile = ($j -eq $files.Count - 1) -and ($directories.Count -eq 0) -and ($nodeModules.Count -eq 0)
        $filePrefix = if ($isLastFile) { "$Prefix└── " } else { "$Prefix├── " }
        Write-Output ("$filePrefix" + $file.Name)
    }

    # List node_modules folder if it exists
    if ($nodeModules) {
        $isLastDir = ($directories.Count -eq 0)
        $nodeModulesPrefix = if ($isLastDir) { "$Prefix└── " } else { "$Prefix├── " }
        Write-Output ("$nodeModulesPrefix" + "node_modules")
    }

    # Iterate through each directory
    for ($i = 0; $i -lt $directories.Count; $i++) {
        $directory = $directories[$i]
        $isLast = ($i -eq $directories.Count - 1)
        $currentPrefix = if ($isLast) { "$Prefix└── " } else { "$Prefix├── " }
        
        # Print the directory name with indentation
        Write-Output ("$currentPrefix" + $directory.Name)

        # Recursive call to process subdirectories
        $newPrefix = if ($isLast) { "$Prefix    " } else { "$Prefix│   " }
        Get-FolderTree -Path $directory.FullName -Prefix $newPrefix -IncludeCs $IncludeCs -IncludeMd $IncludeMd -AllFiles $AllFiles
    }
}

# Specify the root path as the current working directory
$rootPath = Get-Location
$rootName = Split-Path -Leaf $rootPath

# Print the root directory name
Write-Output $rootName
Write-Output "│"

# Determine if all files should be included
$AllFiles = $false
if ($All) {
    $AllFiles = $true
    $IncludeCs = $false
    $IncludeMd = $false
}

# Start the tree structure generation
Get-FolderTree -Path $rootPath -IncludeCs $IncludeCs -IncludeMd $IncludeMd -AllFiles $AllFiles
