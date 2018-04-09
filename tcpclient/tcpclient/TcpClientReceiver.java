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
         int test =0; //Just for debugging purposes
         float temp1 = 0;
         float temp2;
         boolean send = true;
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
         //test++;
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
            if (counter == 1 || counter == 2){
            System.out.println(displayFloat);
            //System.out.println(test);
            //This will print stuff like 1,2,160016,160017
            send = !send;
            if (send){
              temp2 = displayFloat;
              System.out.println("The first number is "+temp1 +" and the second one is " + temp2);
              //Process process = new ProcessBuilder("C:\\Users\\ROC-HCI-1\\Downloads\\HEART-20170925T165908Z-001\\HEART\\tmpClient\\tmpClient\\bin\\Debug\\tmpClient.exe","temp1","temp2").start();
            }
            temp1 = displayFloat;
          }
         }
         
        }
        //clientSocket.close();
    }
    catch(Exception ex)
    {

    }
}
}
