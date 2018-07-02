# Counter-Strike-Memory-Manipulation
___
### Horrible Code, Horrible UI, Horrible Security and one hell of a "cheat".

#### Disclaimer: If someone sold you this code, refund your money immediately. This is a free product, and shall not be charged for in any circumstance.

Entrypoint is Form1 where it spawns a LoginForm and you log in to the server. You can change this, and if you don't, then you won't be able to run it. Basically, the code and the security is absolutely horrible, and practically, you can't use it.

That being said, the code is a mess and not refactored from where it worked the first time. Although it does work, it's horribly inefficient in terms of CPU usage and most definitely memory usage. No objects are disposed which isn't suitable for long term usage. (The garbage collector isn't even mentioned once lol)

This means that when you use this, you're going to find bugs, and you're not going to be a happy camper.

### For those who want to learn

If you're new to counter strike memory manipulation, my code can teach you a few things, but you need to keep this in mind, that I didn't intend for this to be a tutorial or any type of learnable experience. The code is, like I said, a mess. But maybe you'll find a way to read the code. Mostly, the structs are bad, and oftenly mixed up.

Oh and I almost forgot, the pointer-offsets are obviously wrong. My class is derived from the common [HazeDumper](https://github.com/frk1/hazedumper).

## Sources

I borrowed some code from other sources such as a random C# BSP Parser, as well as some algorithms for W2S. This was all found on [unknowncheats.me](http://unknowncheats.me/)
