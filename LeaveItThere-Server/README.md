1. Click the green **Use this template** > **Create a new repository** button at the top right of this page
2. Use something like GitBash to clone your new repo into a folder on your computer (https://git-scm.com/downloads) or download it manually with **Code > Download ZIP**
   * Make sure you are cloning **YOUR** new repo, not the example repo itself
3. Rename the following from _LeaveItThereServer_ to your new mod name:
    * Folder the project is in
    * **.csproj** file
    * **.sln** file
4. Open the **.sln** file with a text editor, CTRL+F for _LeaveItThereServer_ and replace it with your new mod name
5. Open the **.csproj** file with a text editor, CTRL+F for _LeaveItThereServer_ and replace it with your new mod name
6. Open your solution by double clicking your **.sln** file, double click **Plugin.cs**
7. Press CTRL + Shift + F, click Replace in Files
    * make sure **Look in** is set to **Entire solution**
    * in Find field, enter: _LeaveItThereServer_
    * in Replace field, enter your new mod name
    * click Replace All in bottom right, click yes if prompted
8. Update Metdata.cs file with correct metadata for your mod