# SignalRChat
This was a prototype testing repository I used when developing and learning SignalR to create the live chat feature in the WPF application.
The chat uses MVVM and very basic UI, the main goal was to learn and understand the SignalR data pipeline for sending and receiving objects in real time.

## Background/Considerations

SignalR is a library that can add real-time web functionality to apps. The idea was to create a real-time chat application for clients to use to chat to collegues that belonged to the same databases. This way clinicians would be able to communicate simply within the EMR software.

The chat hub would also have to be scaleable as for the prototype that I would be working on would just handle chat functionality. This would include sending text, clickable links, and images. In future iterations the chat app would be able to send links to patient records that can be easily accessed by clinicians and group chat features. 

## Demo
Login to the App. This login was only made to visualize how you could only chat with other users in your database. Authentication was handled in the main EMR application. 

![login](/imgs/login.png)

---
Empty chat between two users on the same database: Timmy and Bryan. The bottom left shows the identity of the current user. The left shows users you can chat with, and online status. The top shows explicity who you are chatting with. 
![bryan empty chat](imgs/bryanemptychat.png)

![timmy chat](imgs/emptychat.png)

---
Timmy says hello to Bryan. 

![timmy says hi](imgs/timmy%20says%20hi.png)

Bryan Receives message notification

![bryan gets notif](imgs/message%20from%20timmy.png)

---
Bryan Reads the message

![bryan read message](imgs/bryan%20read%20message.png)

Timmy receives read receipt from Bryan

![timmy gets receipt](imgs/bryan%20opens%20timmys%20msg.png)

---

Bryan wants to show timmy a cool image 

![bryan send pic](imgs/check%20out%20pic.png)

---

Timmy receives the picture

![timmy gets pic](imgs/receive%20pic.png)
