WeakDictionary
==============

Why Creating A WeakDictionary?
--------------
[WeakDictionary] is quite common implementation. It is useful to avoid memory leak when you build third party system that use resource references but should not owned them. That's why [ConditionalWeakTable] has been added in .NET Framework 4. Unfortunately, Unity cannot use [ConditionalWeakTable] due his Mono version. [WeakDictionary] has been created this reason.

How Should I Use It?
--------------
WeakDictionary is really easy to use. It's based on the exact same syntax as the basic [Dictionary] coming from the "*System.Collections.Generic*" namespace of .NET. You simply have to import the content of this GitHub repository in your project.

	using Com.EpixCode.Util.WeakReference.WeakDictionary

	WeakDictionary<object, string> myWeakDictionary = new WeakDictionary<object, string>();
    myWeakDictionary.Add(myObject, "myString");

How could I Get More Info?
--------------
Get more info at [www.EpixCode.com](www.epixcode.com)