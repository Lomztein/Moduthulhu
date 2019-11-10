# Moduthulhu - Modular Discord Bot

Introducing Moduthulhu 2.0! The all new slightly more thought out, slightly less overly complicated bot core!

The 'Moduthulhu - Modular Discord Bot' is a Discord bot core framework build on the [Discord.NET API Wrapper by RougeException](https://github.com/RogueException/Discord.Net). This project intends to create a foundation framework on which more front-end bot functionality can be added through the runtime loading of functionality-containing Plugins. While there exists a few standard plugins which are enabled by default, they provide no real functionality. These standard plugins will be further outlined later.

This bot is written in C# and targets the .NET Core 2.1 LTS framework, and is fully dockerizable and may be pulled from [Docker Hub](https://hub.docker.com/repository/docker/lomztein/moduthulhu) with the command `docker pull lomztein/moduthulhu:latest`, however do notice that it cannot just run out of the box, as it additionally needs a PostgreSQL database to store information on, as well as a mounted volume to read client configuration data as well as store error logs on. Basic familiarity with Docker is recommended, as that's about what I had when I dockerized it. C:

I personally run the bot on a Ubuntu 16.04 server using Docker, and it works perfectly well!

## Disclaimer: Currently in development:

While the core framework is fully functional and unlikely to see breaking changes any time soon, there may be additions and functionality changes as development progresses. It is perfectly possible to write plugins at this point in time, but it cannot be fully guaranteed that they will remain compatable with future versions.

## So what can it do?

### Core framework

 * Runtime loading of plugin functionality, with the ability to toggle them on and off on a per-server basis. 
 * A simple plugin interface that is easy to develop with, and a base class for easier writing of new modules.
 * Inter-plugin communication through a message system, which allows any plugin to call a registered action or function from another plugin.
 * Easy-to-use build-in configuration system that allows for the registration of configuration options, which may then be exposed through any means imaginable.
 * On-the-fly error handling that catches anything that goes wrong in a plugin and prints the full stacktrace to a file, while the bot keeps living.
 * PostgreSQL database support used for both data and config storage, as well as whatever you may need.
 * Consent Assertion that keeps track of whether or not users all the storage of personal data in the bots database.
    
That, and more is included in the core framework, with more features coming!

### Standard plugins

As mentioned previously, the core framework includes some standard plugins which are all enabled by default, as well as some which are critical and cannot be disabled. These are as follows:

 * Command Root: Plugin for managing commands. Any plugin may add commands to it using the aforementioned messaging system.
 * Plugin Manager: Plugin for enabling and disabling individual plugins, as well as display data about them.
 * Configuration: Plugin that exposes registered configuration options through commands.
 * Consent: Simple plugin that allows users to toggle whether or not they consent to storage of their personal data, in accordance with GDPR.
 * Standard Commands: Plugin that adds the standard commands from the command framework. May be disabled.
 * Logger: Simple plugin that prints out any happenings to Standard Output, used primarily for debugging. Recommended to be disabled if you have privacy concerns.
 * Administrator: Plugin that allows for the basic administration of the bot client, unneccesary for most users and may be disabled.

## Creating new plugins

If you desire to contribute to the bots available functionality by creating your own plugin, you can quite easily do so. After all, the framework is designed specifically to allow for that. There are a few prerequisites before you can do so, such as:

* An IDE, such as Microsoft Visual Studio or Visual Studio Code. Anything should do as long as it can compile .NET Core.
* The .NET Core 2.1 LTS framework SDK, which is required for any .NET Core based applications to be compiled.
* Either a copy of the source code from here, or the assemblies downloaded through NuGet for reference.

That should be the basics. Now to create an actual plugin.

As mentioned previously, the most basic element you should be aware of is the "IPlugin" interface, as this is the interface between plugin and bot framework. However, it isn't really neccesary to worry about it, as it is much easier to work with the "PluginBase" abstract class, the default implementation of IPlugin. Inheriting from PluginBase will provide you with an easy foundation to work with, as well as a bunch of build in utility methods.  Additionally, a Descriptor attribute is required to be added to the class, as it is used to define the plugins author, name, as well as optionally a description and a version. An optional Source attribute may also be added, which contains links to an authors website, a source repository, and a link to where the plugins .dll file may be downloaded for patching.

### Here is an empty class that inherits from ModuleBase:

```cs
[Descriptor ("Alan Smithee", "Example Plugin")]
class ExamplePlugin : PluginBase {

    public override void Initialize() {
        throw new NotImplementedException ();
    }

    public override void Shutdown() {
        throw new NotImplementedException ();
    }
}
```
Everything is catagorised into borderline overdone namespaces, so you're gonna need a few `using`. The core namespace is `Lomztein.Moduthulhu.Core`, which also contains a few sub-namespaces.

There are a few more members you can use, including two other Initialize functions that calls at different time during setup. You can also add the Dependancy attribute to declare that your plugin requires a certain other plugin to be able to function.

You have access to a GuildHandler through IPlugin, which contains all Discord events defined by Discord.NET, however they only fire for the specific Discord server that the GuildHandler is tied to. Each individual server that the bot is connected to has its own instance of a GuildHandler, as well as any plugins that may be enabled on the server. Through the GuildHandler you have access to other tools, such as the PluginMessenger, the PluginManager and the PluginConfig classes. These do as following:

 * PluginMessenger: Handles registering and calling of cross-plugin messages, so as to allow plugins to communicate without being strictly coupled together. Attempting to call an unregistered action/function returns displays a warning message in console and returns null/default.
 
Registering an action/function may look like this: `RegisterMessageAction ("Name", (x) => Method (x))`.
Calling a registered action/function would then look like: `SendMessage ("PluginAuthor-PluginName", "Name", value)`

Familiarity with delegates and lambda expressions in C# is recommended, but not required if you just need to do simple stuff.

As may be noticed here, calling a registered action/function requires specifiying the target plugin as well. Additionally, an alternative `RegisterMessageFunction` method may be used instead, which, unlike the previously mentioned, returns a value when the registered function is called.
 
 * PluginManager: Handles the plugins enabled on the server, and contains methods for adding and removing modules from the list of active modules. There is little reason to worry about this, unless you wish to create your own plugin management plugin to replace the standard one.
 
 * PluginConfig: Handles plugin configuration by maintaining and exposing a list of config options, which are registered by individual plugins. Any outside functionality may access this list of config options and implement ways for users to configure plugins through it. By default, this is done by the standard Configuration plugin, however I wish to provide a web-based alternative later down the line.
 
Adding a configuration option looks something like this: `AddConfigInfo ("Name", "Description", new Action<T>(x => _config.SetValue (x))`.

Again, familiarity with delegates and lambda expressions in C# is recommended. Due to limitations of generic classes in C#, this is a bit more verbose than the previously mentioned RegisterMessageAction/Function methods.

Do note that the methods examplified here are shortcut methods from PluginBase.

## IPlugin defined Methods

There are in total four methods defined by IPlugin, all of which are called by PluginManager.

* `PreInitialize ()` - Executed before anything else is done. It is recommended that messages and config options are registered here, as well as any setup that may be neccesary for other plugins to this one.
* `Initialize ()` - The "Default" initialize function, as well as the only one neccesary to worry about if you don't need to interact with other plugins at all.
* `PostInitialize ()` - Executed lastly. It is recommended to use this to process data given by other plugins.
* `Shutdown ()` - Executed when the plugin must shutdown, perhaps to be disabled or reloaded. Use this to revert any changes done to other plugins or the core.

Initialize and Shutdown must be implemented in your plugin class.

### Building your plugins

You're going to want to build your plugins into .dll files for the core to load up and. The simple way to do this is just to build the project as with any other, and moving the primary output file into the build cores Plugins folder. This should be fairly straightforward for anyone who've used Visual Studio in the past, albiet a bit of a trivial hassle after a few times.

The slightly more advanced but easier once set up method is by using post-build commands. Right-click on your project in the solution explorer, click to "Properties", and go to the Build Events tab. To automatically copy the build module .dll, add this line to post-build event: `xcopy "$(TargetPath)" "$(SolutionDir)Core\bin\Debug\netcoreapp2.1\Podules\" /y`

This will automatically copy the plugin into the given folder, which in this case is the default folder that VS builds to when you run the core project in the default Debug configuration. You can change the output path to whatever you want, but this should make it easier to test since you don't have to manually drag files around. Additionally, you can add more lines if you need it copied to different places or you perhaps need some additional files from the build, such as required assemblies or prerequisite modules.

You can also add another xcopy line that copies the symbol files from compilation, which will make it much easier to debug your modules. They must be placed in the bots root folder, so a command line for this would look something like
`xcopy "$(TargetDir)$(TargetFileName).pdb" "$(SolutionDir)Core\bin\Debug\netcoreapp2.1\" /y`

### Loading your plugins

When the bot framework launches, it loads all .dll files within the ./Data/Modules folder and caches all IPlugin based types found within. These are then stored as plugins available for servers to enable.
