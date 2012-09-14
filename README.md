#MineProxy.Net - A .net Minecraft Proxy#

This is the project page of my Minecraft Proxy. It isdesigned to support as many different versions of minecraft as possible with extended compatibility features.

It is a fundamental feature to join with a different minecraft version to he proxy then the underlying server supports. It is great to give users the possibility to join with a newer version, until the backend server is updated.

Because of the current design of the protocol the backend server must be in offline mode. the proxy supports payer authentification, so if you deny players to directly join into your backend server (e.g. using a internal ip) this is no security risk.

##How to clone this repository##

Because this repository uses submodules the syntax is slightly different

<code>
	git clone --recursive git://github.com/pdelvo/Pdelvo.Minecraft.Proxy.git
</code>

Also feel free to fork my code and feel free to send me pull requests!

##How to build##

You can either open the solution file in Visual Studio 2012 or you can use the msbuild command line utility to build the code.

<code>
msbuild /p:Configuration=Debug /p:Platform="Any CPU"
</code>

##Issues##

When you found any issues, bugs, feature requests feel free to report them to me and I will have alook at them!

##License##

This Code is licensed under the [MIT license](http://www.opensource.org/licenses/mit-license.php/)

##Dependencies##

Here is a list of libraries that are used by this software

* [log4net](http://logging.apache.org/log4net/), license: [Apache License, Version 2.0](http://logging.apache.org/log4net/license.html)
* [TPL Dataflow](http://msdn.microsoft.com/en-us/devlabs/gg585582.aspx)
* [.Net Framework 4.5](http://www.microsoft.com/net/)