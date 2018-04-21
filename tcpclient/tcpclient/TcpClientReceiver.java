  import java.io.BufferedReader;
  import java.io.InputStreamReader;
  import java.io.IOException;
  import java.net.Socket;
  import java.io.*;
  import java.nio.charset.StandardCharsets;
  import java.nio.ByteOrder;
  import java.nio.ByteBuffer;


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
         File dir = new File ("C://Users//ROC-HCI-1//Downloads//HEART-20170925T165908Z-001//HEART//tmpClient//tmpClient//bin//Debug");
         String[] argument = new String[3];
         //argument[0] = "notepad.exe";
         //argument[0] = "C:\\Users\\ROC-HCI-1\\Downloads\\HEART-20170925T165908Z-001\\HEART\\tmpClient\\tmpClient\\bin\\Debug\\tmpClient.exe";
	argument[0] = "C:\\Users\\Ania Busza\\Documents\\GitHub\\HEART\\tcpclient\\tcpclient";
         Runtime runtime = Runtime.getRuntime();

        try
        {
        //create input stream
        BufferedReader inFromUser = new BufferedReader(new InputStreamReader(System.in));
        //create client socket, connect to server
        Socket clientSocket = new Socket("localhost",50041);
        //create output stream attached to socket
        DataOutputStream outToServer = new DataOutputStream(clientSocket.getOutputStream());
        //create input stream attached to socket
        DataInputStream inFromServer = new DataInputStream(clientSocket.getInputStream());
		
		    ByteBuffer dataBuf = ByteBuffer.allocate(12);
        //send line to server
        //outToServer.writeBytes(temp);

       //read line from server
        //displayBytes = inFromServer.readLine();
        while(true)
        {
	       byte[] tempBytes = new byte[4];
  		   inFromServer.read(tempBytes);
  		   dataBuf.put(tempBytes);
  		   dataBuf.order(ByteOrder.LITTLE_ENDIAN);
  		   displayFloat = dataBuf.getFloat(0);
  		   dataBuf.clear();
  		   dataBuf.order(ByteOrder.BIG_ENDIAN);
		      //displayFloat = inFromServer.readFloat();
         counter++;
         //test++;
         if(valid){
          if(counter % 16 == 0){
            valid = false;
            counter = 0;
            
          }
         }else{
          if(counter % 15984 == 0){
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
              argument[2] = Float.toString(temp2);
              System.out.println("The first number is "+temp1 +" and the second one is " + temp2);
              System.out.println(argument[1] + " hhhhh "+ argument[2]);

              //Process process = new ProcessBuilder(argument).start();
              /*try{
              Process p = runtime.exec(new String[]{"tmpClient.exe","0.5","1.0"}, null , dir);
              }catch(IOException e){
                e.printStackTrace();
              }*/
            }
            temp1 = displayFloat;
            argument[1] = Float.toString(temp1);
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
