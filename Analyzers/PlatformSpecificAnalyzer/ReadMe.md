PlatformSpecific
==================



VIDEO [3min]: https://github.com/ljw1004/blog/raw/master/Analyzers/PlatformSpecificAnalyzer/ReadMe.mp4
https://github.com/ljw1004/blog/raw/master/Analyzers/PlatformSpecificAnalyzer/Screenshot.png

When you're writing a UWP app, it's hard to know whether a given API is
platform-specific (and hence needs an "adaptivity check") or if it's part
of core UWP. All you can do is either read the docs rigorously, or run your
code on all different platforms to see if it crashes.

This analyzer fixes that problem.

If ever your code calls into a platform-specific API, this analyzer verifies
that you've done an adaptivity check around it -- if you haven't then it reports
a warning. It also provides a handy "quick-fix" to insert the correct check,
by pressing Ctrl+Dot or clicking on the lightbulb.

For advanced scenarios, this analyzer also supports "transitivity" -- you can declare
that an entire method of yours is platform-specific, eliminating the need
for any checks inside, but forcing you to check before invoking the method.
It also supports "feature flags" -- you can declare that a given field
or property embodies the result of calling ApiInformation.IsTypePresent, so
that checking this field is as good as calling it directly.

The analyzer does have a few false positives, in cases where you've written your
own idiosyncratic style of adaptive checks that are technically valid but aren't
conventional. And it has quite a few false negatives, where your method does
some the wrong adaptivity check. That's because there are too many reasonable ways
to do adaptivity check and the analyzer can't possibly reason about them all, so it
errs on the side of permissiveness.

In all, the analyzer aims to be a helper so you don't simply forget your adaptivity checks.
It doesn't aim to be a complete rigorous proof that your entire app is adaptivity-safe.



TECHNICAL SPECIFICATION

Some platform methods come from a specific UWP platform extension SDK.
And some methods might have the [PlatformSpecific] attribute on them.
And some fields/properties might have the [PlatformSpecific] attribute too.

This analyzer checks that any invocation of a method that's in Windows.* namespace
but outside the common UWP platform, and any invocation of a method with the
[PlatformSpecific] attribute on it, either (1) is in a method/class/assembly
marked as [PlatformSpecific], or (2) is "properly guarded" as defined below.


PROPERLY GUARDED

Here are some examples of code that, technically speaking, won't throw exceptions
at runtime due to lack of adaptivity checks:

Case 1: If ApiInformation.IsTypePresent("xyz") Then xyz.f()

Case 2: Dim b = ApiInformation.IsTypePresent(xyz)
        If b Then xyz.f()

Case 3: If Not ApiInformation.IsTypePresent(xyz) Then Return
        xyz.f()

Case 4: If Not ApiInformation.IsTypePresent(xyz) Then
        Else
           xyz.f()
        End If

Case 5: If GlobalState.FeatureAllowed Then xyz.f()
        where the FeatureAllowed field/property Is Like "b" above

Case 6: Select Case False
           Case ApiInformation.IsTypePresent(xyz) :
           Case Else : xyz.f()
         End Select

Case 7: If(ApiInformation.IsTypePresent(xyz), xyz.f(), 0)

In an ideal world I'd like to have dataflow ability, and check whether the invocationExpression
is reachable via a path where none of the conditions along the way have data flowing into
then that might be influenced by ApiInformation.IsTypePresent or by a global field/property
with [PlatformSpecific] attribute on it. In the absence of dataflow, I'll fall back on a heuristic...
        
Rejected heuristic: walk backwards from the current InvocationExpression,
up through all syntacticaly preceding expressions, and see if any of them
mentioned ApiInformation.IsTypePresent or a global field/property. This
would have almost no false positives (except in case of GoTo and Do/Loop Until cases)
but I think has too many false negatives.
        
Chosen heuristic: enforce the coding style that you should keep things
simple. You must either have this call to xyz.f() or a Return statement inside the
positive branch of an "If" block with a proper guard in its conditional. (Not even an
"Else If" block). A proper guard is either an invocation of any method inside
a type called ApiInformation, or an access of a field/property that has
the [PlatformSpecific] attribute on it.



LIMITATIONS

Backlog: This analyzer only works with methods. It only examines user-written
methods for their contents, and it only looks for invocations of platform-specific
methods. It should be able to look at all user-written code, and should look for
all kinds of member access.

Limitation: This analyzer doesn't have knowledge of *which* platform is specific.
Its designed role is merely as a safeguard, to remind you that you should be guarding.
It won't help in cases where you're already using some other specific guard but has
platform-specific member accesses that aren't covered by that other specific guard.
(In any case, users typically use their "sentinal canary" undocumented knowledge
that if one type is present then a whole load of other types are also present).

Backlog: This analyzer doesn't deal with UWP "min-version". It should eventually,
once a new version of UWP has been released.

Limitation: This analyzer doesn't and can't work through lambdas.
In other words it can't track whether a delegate contains platform-specific member
accesses. It can't because to do so you'd need a type system like
List<Action[PlatformSpecific]>, and the CLR type system doesn't do that.

Backlog: This analyzer doesn't detect when you invoke a method inside
a class with [PlatformSpecific]. Nor an assembly.

Backlog: It doesn't recognize [PlatformSpecific] on parameters.
