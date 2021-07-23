# Distance mod template

Use this repository as a base to create your own Distance mods.

# Using the template
On github, click the `Use this template` button to create a new repository based on this one

Once the repository is created, be sure to clone it locally **with submodules**:
```sh
git clone --recurse-submodules -j8 <your project .git url>
```

# Setting up the project
After opening the project in Visual Studio, be sure to rename the project files while respecting the following naming rules:

- Rename `Mod.Template` and change `Template` to `<your mod name>`
- Rename `Mod.Template.Content` to the name of the previous project followed by `.Content`
- Rename the solution file to the same as the main project file

After renaming those files, open your Content project then go to `Mod` and open the `mod.json` file. This file tells the Centrifuge mod loader how to use your mod, be sure to fill the required information accordingly.

Finally, in the main project, open `Mod.cs` and change the mod unique identifier at the top of the file.
