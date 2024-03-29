# TranslateResxToAndroidTool

Extremely simple tool for transforming ResX files into Android xml files with string resources.
Below you can find some steps to make it handy to use.

## Install/Uninstall

`dotnet tool install --global --add-source .\nupkg TranslateResxToAndroidTool`

`dotnet tool uninstall --global TranslateResxToAndroidTool`

## External Tool Config

1. Tools -> External Tools

![](https://raw.githubusercontent.com/Drutol/TranslateResxToAndroidTool/master/.github/externalToolConfig.png)

### Command

`cmd.exe`

### Arguments

`/c resxtoandroid $(ItemPath) -autoCreate -outputFromMetadata`

### Initial Directory

`$(SolutionDir)`

## Context Menu Customization

![](https://raw.githubusercontent.com/Drutol/TranslateResxToAndroidTool/master/.github/contextMenu.png)

1. Tools -> Customize... -> Commands
2. Select Context Menu.
3. Pick `Project and Solution Context Menus | Item`
4. Add Command...
5. Tools
6. Pick `External Command #` (whichever number your command has)

## Output from metadata

Your Resx file has to contain additional keyt Meta_TargetAndroidFile point to output file. (relative to $(SolutionDir))
