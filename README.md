# Vuforia-Univ-Nantes: Immersive Data Visualization
*Authors: AL ANAISSY Caren and LOMBAERDE Julien and HARTLEY Marc*

## Installation
Please make sure you have [Unity](www.unity.com) installed on your computer. In this project, we are using [Unity 2019.4.18](https://unity3d.com/get-unity/download/archive). Older version will not be compatible.  

This project works with the powerful [Vuforia Engine](https://developer.vuforia.com/). Our version is the [Vuforia SDK 9.6](https://developer.vuforia.com/downloads/sdk).

### Installing Vuforia
Be sure to have Git installed on your computer: open your terminal or console and type `git --version`  
If there is an error, please visit https://git-scm.com/downloads to download it.  

#### Method 1 (before loading this project)
Now create an empty project on Unity (that you will probably delete in few minutes).  
On the top menu, go to "Window" > "Asset Store". This will open the asset store in your Unity Editor.  
In the search field, type "Vuforia Engine" and click on the corresponding item of the Store.  
Click on "Download". When the downloading is finished, click "Import".  
Accept "Install" and "Update" every time it is requested. At last, a window should open with a tree of assets to import, simply click "Import".  
You can delete the empty project and download/open this one.

#### Method 2
Visit the [SDK download page of Vuforia](https://developer.vuforia.com/downloads/sdk) and select "Add Vuforia Engine to a Unity Project or upgrade to the latest version".  
*You might need to sign up/sign in Vuforia to get access to the file, but you might need to get an account anyway for the next part of the installation*  
Open the project in the Unity Editor, then execute from your explorer the file that you have just downloaded.  
A window will open in Unity with 2 files. Press import and wait for the download to finish.  
You are done.

---
You can check that Vuforia is installed by creating a new object in the scene. If you open the "GameObject" top-menu, you must see an item "Vuforia Engine".

### Configuring the project
Once Vuforia is downloaded and imported in your project, be sure that all the objects have the good components:
- "Tag0" should have the components "Image Target Behaviour" and "Default Trackable Event Handler",
- "ARCamera" must have the component "Vuforia Behaviour" and "Default Initialization Error Handler".

If any of them are missing, just add them and keep the default parameters.  

We have two last steps to do before finally being able to lauch the project on your computer.

#### Configuring Vuforia
- Select the ARCamera object, click on "Open Vuforia Engine configuration" from the inspector.
- You must indicate the app licence key.
    - Either you simply ask me (Marc) to share mine.
    - Or you access to Vuforia [Licence Manager](https://developer.vuforia.com/vui/develop/licenses) (an account is required, but it's free). You create a "Development Key" and copy-paste the given key in Unity.
- Increase the number of "Max simultaneous tracked image" to the number of different trackers you want. In this project, we only use 1 tracker at the time, but you can increase to a higher number if you want.

#### Configure the marker
- Select the Tag0 object.
- In the inspector, find the "Image" parameter and pass the image you want to track. In this project we are using AruCo, so you might want to pass an AruCo marker image.

## Run the project
Finally, here we are!  
You can now run the project in Unity. You should not get any error in the console, only debug messages from the Vuforia Engine.   

## Play it on smartphone
### Real smartphone
The project should be compatible with Android, iOS and Windows phones and tablets.  
To create the installation file for your phone, go to "File" > "Build Settings..." from the Unity Editor.  
Select your desired target (you must have installed the module of your desired platform) and press "Build".  
Connect your phone/tablet to your computer and put the installation file just created in your phone storage. Access it from your phone and execute it to install it.  
Your phone will tell you the application is unsafe, but install anyway.  
Everything should work perfectly!

### Emulator 
*(I only know the procedure for Android emulators, and will take [BlueStacks](https://www.bluestacks.com/) as example)*
Open BlueStacks, go to the settings > "Preferences" and check "ADB" (Android Debug Bridge). Save.  
Go back to your Unity Project, click on "File" > "Build settings...", select "Android". In the parameters, change the "Run Device" to whatever looks like a device name (might need to click "Refresh" to find it).  
Click on "Build and Run", wait for the loading, then it will be automatically launched on BlueStacks.  

For the future, this action can be done with "Ctrl + B".

## Using the app
Run the app on whatever device you want and make sure that the marker is detected (a scatterplot should appear on the marker).  
Our goal is to focus on the selection of multiple points in a volume. The unique action is then to select one point by pressing on it or select multiple ones by drawing a circle around multiple points.  
For deselecting point, press the "Deselect" button and press or surround the points you want to deselect.  
Statistics are shown at the top-left of your screen for the selected points.
