# Install/Uninstall

dotnet tool install --global --add-source .\nupkg TranslateResxToAndroidTool

dotnet tool uninstall --global TranslateResxToAndroidTool

#ExternalToolConfig

## Command

cmd.exe

## Arguments

/c resxtoandroid $(ItemPath) -autoCreate -outputFromMetadata

## Initial Directory

$(SolutionDir)

# Context Menu Customization

1. Tools -> Customize... -> Commands
2. Select Context Menu.
3. Pick "Project and Solution Context Menus | Item"
4. Add Command...
5. Tools
6. External Command # (whichever number your command has)

#Output from metadata

Your Resx file has to contain additional keyt Meta_TargetAndroidFile point to output file. (relative to $(SolutionDir))
