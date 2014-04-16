WeakDictionary
==============

Why creating a WeakDictionary implementation?
--------------
**WeakDictionary** is quite common implementation. It is useful to avoid memory leaks when you build third party system that use resource references but should never owned them. That's why *ConditionalWeakTable* has been added in .NET Framework 4. Unfortunately, Unity cannot use *ConditionalWeakTable* due to the Mono version it use. **WeakDictionary** has been created to fix this problem.

How should use it?
--------------
WeakDictionary is dead simple to use. It's based on the exact same syntax as the basic *Dictionary* coming from the "*System.Collections.Generic*" namespace of .NET. You simply have to import the content of this GitHub repository in your project.

	using Com.EpixCode.Util.WeakReference.WeakDictionary

	WeakDictionary<object, string> myWeakDictionary = new WeakDictionary<object, string>();
    myWeakDictionary.Add(myObject, "myString");

How could I get more information?
--------------
Get more info at [www.EpixCode.com](www.epixcode.com)