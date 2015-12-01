PlatformSpecific
==================

This analyzer helps make sure that your Win10 apps are *universal* - will run on all Win10 devices.

* References > Manage NuGet References > install [PlatformSpecific.Analyzer](https://www.nuget.org/packages/PlatformSpecific.Analyzer)
* [Watch training video](https://github.com/ljw1004/blog/raw/master/Analyzers/PlatformSpecificAnalyzer/ReadMe.mp4) [3mins]

![screenshot](https://raw.githubusercontent.com/ljw1004/blog/master/Analyzers/PlatformSpecificAnalyzer/Screenshot.png)


Explanation
-------------

When you're writing a UWP app, it's hard to know whether a given API is
platform-specific or version-specific (and hence needs an "adaptivity check") or if it's part
of core UWP. All you can do is either read the docs rigorously, or run your
code on all different platforms to see if it crashes.

This analyzer fixes that problem.

If ever your code calls into a platform- or version-specific API, this analyzer verifies
that you've done an adaptivity check around it -- if you haven't then it reports
a warning. It also provides a handy "quick-fix" to insert the correct check,
by pressing Ctrl+Dot or clicking on the lightbulb.

For advanced scenarios, this analyzer also supports "transitivity" -- you can declare
that an entire method of yours is platform-specific, eliminating the need
for any checks inside, but forcing you to check before invoking your method.
It also supports "feature flags" -- you can declare that a given field
or property embodies the result of calling ApiInformation.IsTypePresent, so
that checking this field is as good as calling IsTypePresent directly.

The analyzer does have a few false positives, in cases where you've written your
own idiosyncratic style of adaptive checks that are technically valid but aren't
conventional. And it has quite a few false negatives, where your method does
an adaptivity check but checks for the wrong thing. That's because there are too many
reasonable ways to do adaptivity check and the analyzer can't possibly reason about
them all, so it errs on the side of permissiveness.

In the end, the analyzer makes sure you're doing adaptivity in a good standard
easy-to-read coding pattern. Its goal is to make sure you didn't flat-out forget
to do the adaptivity checks you need. It doesn't try to check that you've done the
exact right check, and it doesn't aim to be a complete rigorous proof that your
entire app is adaptivity-safe.



Technical specification
-------------------------

Some platform methods come from a specific UWP platform extension SDK,
or from a version of UWP that's higher than the TargetPlatformMinVersion in your vbproj/csproj.
And some methods might have the [PlatformSpecific] attribute on them.
And some fields/properties might have the [PlatformSpecific] attribute too.

This analyzer checks that any invocation of a method that's in Windows.* namespace
but outside the common UWP platform, and any invocation of a method with a
[*Specific] attribute on it, either (1) is in a method marked as [*Specific],
not necessarily the same same attribute, or (2) is "properly guarded" as defined below.

*Properly Guarded*. You must either have the invocation itself or a Return statement
inside the positive branch of an `If` block whose conditional includes a "proper guard".
A proper guard is either an invocation of any method inside
a type called ApiInformation, or an access of a field/property that has
a `[*Specific]` attribute on it.

The package provides one attribute `[System.Runtime.CompilerServices.PlatformSpecific]`,
and assumes this is the right attribute to add in its quick-fix,
but you can chose to define
your own more descriptive attributes, e.g. `[MyNamespace.XboxSpecific]`, and all
attributes that end in `Specific` are treated as identical by this analyzer.
The code action to insert a codefix will use whatever attribute the code claims to need.

The above discussion is a bit loose about exactly what things are checked.
Places the attribute can go: {methods, properties, fields, constructors}.
WinRT things that might be platform-specific: {methods, property accessors, enum members, constructors, events}.
Expressions that are checked: {invocations, constructions, property accesses, enum member access, AddHandler}.
Expressions within these things are checked: {methods, properties, fields, constructors, C# getter-only
autoprops, autoprop initializers, field initializers}. Everything apart from the last two
(initializers) can have a [*Specific] attribute.




Design decisions
------------------

It's conceptually meaningless to have this kind of analyzer for .NET code. That's because
in UWP apps, all .NET code is bundled along with your app (including the .NET framework itself).
Therefore every .NET API that you reference is by definition present. Now it might have been
helpful to have adaptivity checks for desktop apps, so you can write a single binary that runs
on desktop .NET framework v4.0 but can also call into v4.6 APIs if they're on the machine.
But the only version of the .NET runtime that supports adaptivity is the one used in UWP apps.

It's impossible for an analyzer to know whether adaptivity checks are the right thing.
Example1: I see lots of folks checking for whether the `HardwareButtons` type is present, and if
so inferring that the device is a phone, and therefore changing their UI to be
phone specific. That's wrong (e.g. what happens if a tablet comes out with hardware
buttons?) It's better to change UI based on screen size. Example2: I see lots of folks
hooking up to `HardwareButtons.BackPressed` event. That's wrong: they should hook up
to the universal `SystemNavigationManager.Backpressed` event instead.

It's impossible for an analyzer to know whether you're using the *right* adaptivity
checks. At the most fundamental level, folks will reasonably use "canaries" -- i.e.
tests of whether one type is present, and they infer that a whole other family of
types is present.

That means it's impossible for an analyzer to detect cases where you've written
a guard, but it turns out to have been the wrong one for the API you're using.
All the analyzer can aim to do is remind you that you should be guarding
in cases where you've forgotten completely. (However, at least the idea of `MobileSpecific` /
`DesktopSpecific` / `XboxSpecific` would mitigate this somewhat.)

It's impossible for an analyzer to work through lambdas. For instance, it can't
track whether an `Action` delegate contains platform-specific operations. It can't
do this because you'd need an "effect-based type system" like
`List<Action[PlatformSpecific]>`, and the CLR type system doesn't do that.

Ultimately, the job of compile-time analysis is to ensure that certain
classes of runtime failures won't happen, that class being exceptions at
runtime due to missing adaptivity checks. In cases where this problem is too
hard in general, the compile-time analyzer will constrain the problem,
requiring the user to write code in a pattern that's more amenable to
good analysis. Here are some examples of code that, technically speaking,
won't throw exceptions at runtime due to lack of adaptivity checks:

```vb
If ApiInformation.IsTypePresent("xyz") Then xyz.f()
```

```vb
Dim b = ApiInformation.IsTypePresent(xyz) ' local variable
If b Then xyz.f()
```

```vb
If Not ApiInformation.IsTypePresent(xyz) Then Return
xyz.f()
```

```vb
If Not ApiInformation.IsTypePresent(xyz) Then
   ...
Else
   xyz.f()
End If
```

```vb
If GlobalState.FeatureAllowed Then xyz.f()
```

```vb
Select Case False
   Case ApiInformation.IsTypePresent(xyz) :
   Case Else : xyz.f()
End Select
```

```vb
If(ApiInformation.IsTypePresent(xyz), xyz.f(), 0)
```

```vb
If False Then xyz.f()
```

To make an analyzer that can handle all these, it would need *dataflow ability*. It would
need the ability to check whether a given operation is reachable via a path where
*none* of the conditions along the way have data flowing into them that might be
influenced by `ApiInformation.IsTypePresent` or by a global field/property with
`[PlatformSpecific]` attribute on it.

*However, even within that ideal world, it's still impossible to know whether the "influence"
was correct or not!* For instance, if the result of `ApiInformation.IsTypePresent` gets fed
into an integer which then has arithmetic done on it, or fed into a boolean expression,
or used to control the visibility of a button. Because of this, I think that it's just
not worth going to the effort of dataflow analysis. We have to fall back to heuristics.

One possible heuristic is to walk backwards from the current operation, up through all
lexically preceding expressions, and see if any of them mentioned `ApiInformation.IsTypePresent`
or an appropriately-annotated field/property. This would have almost no false positives
(except in the case of GoTo). But I think it would have too many false negatives.
That's why I instead picked the heuristic explained above in the "Technical specification"
section.

It's not feasible to support marking a class/assembly as PlatformSpecific (with the
implication that all members of it are themselves PlatformSpecific). This would be okay
for pure WinRT types, but .NET allows so much richer things in its types - e.g. user-defined
conversions - so that if the type itself were platform-specific then we'd have to check
a scary-large-number of subtle ways that it and its members might be used.

There isn't a clean good way to handle "else if". In VB it only considers the first "If" condition
out of a series of if/elseif/elseif/else. In C# it looks up all the conditions preceding
the current block. I don't really know which is better. Should it account for "if not ApiPresent"?
Or should it only accept platform-specific operations in a branch whose immediate guard
was good? I don't have a good idea.

I decided against using more specific attributes such as `[MobileSpecific]`, `[DesktopSpecific]`
and so on. That's partly because an API can happily be a member of several different platforms,
but mostly because the whole point of correct adaptivity is that you don't just
say `if (xbox)`. Instead you do an API-by-API check of whether a given API is present.
That said, if the user does chose to use their own attributes, then the analyzer will happily
propagate them in its code-action.

I decided against providing a code-action that updates the project's TargetPlatformMinVersion.
I don't think that's a friendly way to go about adaptivity, and don't want to steer people into it.


