UnityGameTools
===

Collection of various tools we use in Unity3D game development.





Setup 
---

I use Visual Studio to build a separate game DLL file, so I have a slightly different setup. 

I have a separate "Code" directory in my project, and then add this repo into it as a submodule, as follows:

    MyGame/
    +-- Assets/
    	+-- <built assets and code dlls>
    +-- Code/
        +-- UnityGameTools/   <-- this repo
        +-- Game/
   		+-- Game.sln 
   	+-- <other unity directories>

Then inside my *Game.sln*, I add *UnityGameTools/Source.csproj* as an existing project. [Source project file](Source/Unity Game Tools - Source.csproj) is already set up for Visual Studio, to build a DLL automatically into the project's Assets directory, to be picked up by Unity. For more info on how that's set up, check out: [Unity and Visual Studio setup](http://aseparateblog.blogspot.com/2014/06/unity-and-visual-studio-using.html)

Alternatively you should be able to just copy files from [Source](Source) into your game's Assets directory and go from there.





