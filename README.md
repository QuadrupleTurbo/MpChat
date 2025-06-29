![image](https://github.com/user-attachments/assets/f119c9ce-7080-4fbd-a8fa-9dd59e34a924)

# MpChat
**A custom version of the GTA:O chat scaleform!**

# What's "Custom" about it?
Well, compared to the regular GTA:O chat scaleform, this allows you to pretty much have full control over the chat. This resource will run as a very standard chat resource from the get go, however if you're wanting roles and stuff, you're going to have to implement your own logic into it, providing you know C#. Feel free to contribute :slight_smile:

# Features
- Input character limit
- Input history selector using the up/down arrow keys
- Choose the amount of lines displayed in the feed
- Feed fade out delaying
- Message delaying
- Changeable positions of the input and the feed
- A bad word filter
  - It will star the words out like it does in GTA:O
 - An experimental paste feature
   - Disabled by default, it utilises NUI to grab the pasted text to then send to the scaleform, which currently it doesn't work that smoothly

# Known issues
- This will conflict with vMenu's "M" key (I'm not sure on how to prevent this yet)
- The feed history scroll up/down is currently broken
  - This was broken right from the decompilation and I never figured it out, so someone might be able to fix the code in Actionscript

# Exports
| Export                               | Description                                
|-------------------------------------  |-----------------------------|
| AddMessage(string name, string message, int scope, bool team, int color, bool isCensored) | Adds a message to the feed (see examples in code) 
| SetInputPositionOffset(float x, float y) | Changes the offset position of the input 
| SetFeedPositionOffset(float x, float y) | Changes the offset position of the feed             

# Preview
https://github.com/user-attachments/assets/7c657964-80d4-42c6-952b-da2b7857c0b6

# Downloads
You need these two, one is the script, and one is the scaleform:
> [Script Github](https://github.com/QuadrupleTurbo/scaleformeter/releases)
> 
> [Scaleform Github](https://github.com/QuadrupleTurbo/scaleformeter/releases)

# Support
[Discord](https://discord.gg/KJVD73D3Pq)
