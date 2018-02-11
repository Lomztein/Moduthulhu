# Modular Discord Bot

There is a story of a Discord Bot. A bot that became so bloated and so full of useless, poorly planned or poorly executed features that it became little more than MOMS SPAGHETTI.

*Ahem*, sorry.

The point is that my previous Discord Bot, gloriously named Adminthulhu, became kind of a mess of code, and I became sick of having endless issues with it. So here we are now. Welcome to the Modular Discord Bot repository!

So what is this all about, you might be wondering? As the name implies, it is a Discord bot build around the concept of a modular approach to features. Instead of having all features fighting for space in a single codebase, this bot splits features up into modules! Modules are dynamically loaded at runtime from .dll files, and can be written by anyone with a bit of C# experience and this codebase. The reason for the modular approach was a desire to have a single bot with many different features, so that you don't need to flood a server with like ten different bots, each doing a single thing.

This bot is based on the [Discord.NET API Wrapper by RougeException](https://github.com/RogueException/Discord.Net), and is written for the .NET Core multiplatform .NET framework. It will require .NET Core on whatever machine you need to run it on.

## Do notice, however..

This bot is currently in early development, and while you can currently write modules and use the bot, it is subject to change, and I cannot promise that your modules will remain compatable with all future versions of the bot.

## So what can it do?

### Core framework

* Dynamic loading of modules that can does just about anything your heart desires, and possibly more.
    * A simple module interface that connects the module to the core, and a base class for easier writing of new modules.
    * Inter-modular references. Wrote something that references another module? The core loads all module assemblies into memory, so you can talk to any module you want, assuming of course that module wants to talk to *you.*
    * Module metadata, like name and author, or which modules are required for it to work, or if it supports multiserver bots.

* Object based configuration framework with native support for both multi- and singleserver bots!
    * Configuration objects are bound to a path, and will load and save their data from there.
    * Easy support for multiserver bots through the MultiConfig and MultiEntry objects. Hell, they can be used to bind config to anything with an ID, which is pretty much anything in Discord.
    * Easy-to-edit JSON formatted files split into individual files per ID in a directory for MultiConfig objects, and a single file for SingleConfig objects.
    
Well that's all there currently is for the core, but more is coming.

### Standard modules

* Server Messages, a module that sends various messages to chat on certain events!
* Command Root, a module that implements my [Advanced Discord Commands](https://github.com/Lomztein/Advanced-Discord-Commands) library. It should act as a base for any other command-using module.
* Standard Commands, a simple module that adds all standard commands from the arorementioned library.

## Creating new modules

Creating new modules is easy-ish once you've set up. Naturally you're going to have to know how to write a bit of C#, but if not this might be a learning oppertunity! Here's what you're going to need in order to get started:

* An IDE, such as Microsoft Visual Studio or Visual Studio Code. Anything should do as long as it can compile .NET Core and handle modular programming.
* The .NET Core framework, which is required for any .NET Core based applications to run both in an IDE and independantly.
* A copy of the core project, so that you can use it for references. Additionally any other modules you might want to interact with.

That should be the basics. Now to create an actual module.

### Setting up shop
* Open up this repositorys in your IDE, or create a new folder, doesn't really matter as long as you can refer to this in your code.
* Create a new project, something that compiles to a .dll file, like a library project.
* In references, add a reference to Core.dll, this will grant you access to the core framework.
* In references, add references to all modules you want to interact with. Am I repeating myself? :thinking:
* Create a new class, it must implement IModule in one way or another. Inheriting from ModuleBase is the recommended method.

Next, to filling out the actual Module.

While the IModule interface has a fair few members, most of them are implemented by ModuleBase, except for a few critical ones that you must implement yourself. In case your module doesn't inherit from ModuleBase, then you're gonna have to implement all interface members yourself.

### Here is an empty class that inherits from ModuleBase:

```cs
class ExampleModule : ModuleBase {

    public override string Name => "Example Module";
    public override string Description => "This is a module that does absolutely nothing but waste memory.";
    public override string Author => "Alan Smithee";

    public override bool Multiserver => throw new NotImplementedException ();

    public override void Initialize() {
        throw new NotImplementedException ();
    }

    public override void Shutdown() {
        throw new NotImplementedException ();
    }
}
```
Everything is catagorised into borderline overdone namespaces, so you're gonna need a few `using`. The "main" namespace is `Lomztein.ModularDiscordBot.Core`, which also contains a few sub-namespaces.

There are a few more members you can use, including two other Initialize functions that calls at different time during setup, as well as the aforementioned RequiredModules and its relatives RecommendedModules and ConflictingModules. Few more exists, but these are the most important when sitting up the module.

The interface contains a reference to BotClient which is set by the ModuleManager, which is a wrapper for the Discord.NET SocketDiscordClient, which is the object you need in order to register listeners for various Discord events, such as message recieved and user joins.

The interface also contains a reference directly to its parent ModuleManager, which can be used to check if specific other modules are installed.

Four methods are called by ModuleManager:
* `PreInitialize ()` - Executed before anything else is done. 
* `Initialize ()` - Executed after the bot has fully connected to Discord.
* `PostInitialize ()` - Executed after Configure has been called on IConfigurable modules.
* `Shutdown ()` - Executed when the module has to brutally die for some reason. Use this to undo any changes the module might have done, such as event listener registration and command additions.

Initialize and Shutdown must be implemented in the module class.

The sequence which things happen in are subject to change, and the three different initialize steps are mostly for future-proofing.

### Configuring your modules

Once you've written your module, you're going to want to be able to configure it without manually changing the code. This is done with the configuration framework build into the core. You use this through the various Config objects, with two different variations to offer.

At the most basic level, configuration is based on saving objects with identifying keywords. Typically something like `ChannelID: 266882915978313728`, which are saved in JSON and can be loaded whenever needed. It is however recommended that you load and cache all your config entries during initialization.

* `SingleConfig` - Designed to be used for singular servers or items. They do not require an ID'd object, and is bound to a single configuration file.
    * `SingleConfig.GetEntry<T> (string key, T fallback)` returns whatever is at `key` as the type `T`. If nothing is found then it sets the key to `fallback` and returns `fallback`.

* `MultiConfig` - Designed to be used for multiple servers or items. They require an ID'ed object for you to get anything out of them, and they are bound to directories which contain a file for each ID. Most Discord objects has an ID, so you can bind these to pretty much everything you want.
    * `MultiConfig.GetEntry<T> (IEntity<ulong> entity, string key, T fallback)` Works similar to the SingleConfig variant, but requires an ID'd entity, such as a server, a user, or a message object.
    * `MultiConfig.GetEntries<T> (IEnumerable<IEntity<ulong>> entity, string key, T fallback)` Runs the previously mentioned MultiConfig.GetEntry for each IEntity<ulong> in an IEnumerable, and returns them in a `MultiEntry<T>` variable.

* `MultiEntry<T>` - A wrapper for a Dictionary<ulong, T>, which contains the config entries for the key for each ID in the dictionary.
    * `MultiEntry<T>.GetEntry<T> (IEntity<ulong> entity)` returns the object for the given ID'd object.

Usage of MultiConfig and MultiEntry can be seen in [Server Messages Module](ServerMessagesModule/ServerMessagesModule.cs), along with examples of creating a module in general.

Finally, you can implement IConfigurable on anything if to help you out a bit. This interface contains a `Configure` method, which is supposed to contain the code which sets the variables from config. Implementing IConfigurable in a module tells the module manager to automatically configure the module just before Initialize is called, but after PreInitialize.

### Building your modules

You're going to want to build your modules into .dll files for the core to load up and. The simple way to do this is just to build the project as with any other, and moving the primary output file into the build cores Modules folder. This should be fairly straightforward for anyone who've used Visual Studio in the past, albiet a bit of a trivial hassle after a few times.

The slightly more advanced but easier once set up method is by using post-build commands. Right-click on your project in the solution explorer, click to "Properties", and go to the Build Events tab. To automatically copy the build module .dll, add this line to post-build event: `xcopy "$(TargetPath)" "$(SolutionDir)Core\bin\Debug\netcoreapp2.0\Modules\" /y`

This will automatically copy the module into the given folder, which in this case is the default folder that VS builds to when you run the core project in the defualt Debug configuration. You can change the output path to whatever you want, but this should make it easier to test since you don't have to manually drag files around. Additionally, you can add more lines if you need it copied to different places or you perhaps need some additional files from the build, such as required assemblies or prerequisite modules.

You can also add another xcopy line that copies the symbol files from compilation, which will make it much easier to debug your modules. They must be placed in the bots root folder, so a command line for this would look something like
`xcopy "$(TargetDir)$(TargetFileName).pdb" "$(SolutionDir)Core\bin\Debug\netcoreapp2.0\" /y`

### Loading your modules

When the bot launches, it creates an instance of ModuleManager. This manager automatically loads up all .dll files in the Modules folder next to Core.dll, and loads all IModule objects from these .dll files into the ModuleManager. Non-module .dll files are also loaded into memory, which means you can add your own libraries to the folder, and it should work just fine. This also means having multiple modules per .dll file is perfectly viable, however it is recommended that you don't put multiple unrelated modules into a single .dll file, since it'd make it more difficult to manage.

The module folder also contains i small JSON file that lists each module and whether or not they are enabled.

### Versions and compatability

As long as the module implements the same IModule interface that the version of the bot you're running does, then it should work at the basic level no matter what. It's different if the module references parts of the core framework, which is more likely to change between versions. Same applies if the module refers to other modules. There is currently no version-checking system in place, and I'm not sure there will ever be one.

## Finally..

Want your module added to the list of standard modules? Just create a pull request, and I'll look into it.

I have no idea what to name this bot. Currently it's just named "Modular Discord Bot", but that doesn't exactly roll of the tounge. I'd like a different, more radical name. Was considering Moduthulhu, but on the other hand I think my code already has enough eldritch horrors. The point is that new name suggestions are very welcome!
