import java.io.BufferedReader;
  import java.io.InputStreamReader;
  import java.net.Socket;
  import java.io.*;
import java.nio.charset.StandardCharsets;


  public class TcpClientReceiver {


       public static void main(String[] args) {
         String temp;
         float displayFloat;
         int counter = 0;
         boolean valid = true;
        try
        {
        //create input stream
        BufferedReader inFromUser = new BufferedReader(new InputStreamReader(System.in));
        //create client socket, connect to server
        Socket clientSocket = new Socket("localhost",50041);
        //create output stream attached to socket
        DataOutputStream outToServer =
                new DataOutputStream(clientSocket.getOutputStream());




        //create input stream attached to socket
        DataInputStream inFromServer = new DataInputStream(clientSocket.getInputStream());


        //send line to server
        //outToServer.writeBytes(temp);

       //read line from server
        //displayBytes = inFromServer.readLine();

        while(true)
        {
	       displayFloat = inFromServer.readFloat();
         counter++;
         if(valid){
          if(counter % 16 == 0){
            valid = false;
            counter = 0;
          }
         }else{
          if(counter % 16000 == 0){
		//manually set the frequency to 2HZ...
            valid = true;
            counter = 0;
          }
         }
         if(valid){
          System.out.println(displayFloat);
         }
         
        }
        //clientSocket.close();
    }
    catch(Exception ex)
    {

    }
}
}
