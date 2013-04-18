reactivesockets
===============

Implements a very easy to use sockets API based on IObservable. It allows very simple protocol implementations such as:

        var client = new ReactiveClient("127.0.0.1", 1055);
        
        // The parsing of messages is done in a simple Rx query over the receiver observable
        // Note this protocol has a fixed header part containing the payload message length
        // And the message payload itself. Bytes are consumed from the client.Receiver 
        // automatically so its behavior is intuitive.
        IObservable<string> messages = from header in client.Receiver.Buffer(4)
                                       let length = BitConverter.ToInt32(header.ToArray(), 0)
                                       let body = client.Receiver.Take(length)
                                       select Encoding.UTF8.GetString(body.ToEnumerable().ToArray());
        
        // Finally, we subscribe on a background thread to process messages when they are available
        messages.SubscribeOn(TaskPoolScheduler.Default).Subscribe(message => Console.WriteLine(message));
        client.ConnectAsync().Wait();


Creating the server implementation is equally straightforward (this is an echo server for the same message format):

            var server = new ReactiveListener(1055);
            server.Connections.Subscribe(socket =>
            {
                IObservable<string> messages = from header in socket.Receiver.Buffer(4)
                                               let length = BitConverter.ToInt32(header.ToArray(), 0)
                                               let body = socket.Receiver.Take(length)
                                               select Encoding.UTF8.GetString(body.ToEnumerable().ToArray());

                // Echo the incoming message with the same format.
                messages.Subscribe(message =>
                { 
                  var body = encoding.GetBytes(message);
                  var header = BitConverter.GetBytes(body.Length);
                  var payload = header.Concat(body).ToArray();
                  
                  socket.SendAsync(payload).Wait();
                });
            });
  

            server.Start();


Install using: [install-package reactivesockets](https://nuget.org/packages/ReactiveSockets)

This library was inspired by this [forum post](http://social.msdn.microsoft.com/Forums/en/rx/thread/5c62e690-2c8d-4f32-8ec4-5e9b5ea6d2a0) and [blog entry](http://www.cachelog.net/using-reactive-extensions-rx-tpl-for-socket-programming/).
