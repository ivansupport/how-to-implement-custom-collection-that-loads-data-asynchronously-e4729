# How to implement Custom Collection that loads data asynchronously

<p><b>Note:</b> In version <b>18.1</b>, we implemented ready-to-use <a href="https://documentation.devexpress.com/WPF/10803/Controls-and-Libraries/Data-Grid/Binding-to-Data/Binding-to-any-Data-Source-with-Virtual-Sources">Virtual Source</a> classes that allow you to populate <b>GridControl</b> on demand, and if required, support certain data operations (sorting, filtering). Now, this is a recommended approach to populating your grid on demand if <a href="https://documentation.devexpress.com/WPF/9588/Controls-and-Libraries/Data-Grid/Binding-to-Data/Server-Mode">Server Mode</a> components are insufficient.</p>

<p>This examples demonstrates how you can implement a custom IList descendant that provides the capability to load data on demand.</p>

<br/>


