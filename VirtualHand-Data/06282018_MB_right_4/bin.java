import java.io.*;
import java.util.*;

public class bin {

	private Scanner xcanner;
	
    public static void main(String [] args) {

        // The name of the file to open.
        String fileinputName = "CyberGloveData.txt";
        String fileoutputName1 = "5bin.txt";
        String fileoutputName2 = "10bin.txt";
        String line = "what the fuvckc";
        String[] temp = null;

        String CGData = "Testing";
        String CGData = null;


        try{
            FileReader fileReader = new FileReader(fileinputName);

            BufferedReader bufferedReader = new BufferedReader(fileReader);
            // Assume default encoding.
            FileWriter fileWriter1 = new FileWriter(fileoutputName1);
            FileWriter fileWriter2 = new FileWriter(fileoutputName2);
            // Always wrap FileWriter in BufferedWriter.
            BufferedWriter bufferedWriter1 = new BufferedWriter(fileWriter1);
            BufferedWriter bufferedWriter2 = new BufferedWriter(fileWriter2);
            
            line = bufferedReader.readLine();
            
            
            String sCurrentLine;

            if((sCurrentLine = bufferedReader.readLine()) < -0.25){
            	bufferedWriter1.write(-0.2);
            	bufferedWriter1.newLine();
            }else if((sCurrentLine = bufferedReader.readLine()) < -0.1 && (sCurrentLine = bufferedReader.readLine()) > -0.25){
                bufferedWriter1.write(-0.1);
                bufferedWriter1.newLine();
            }else if((sCurrentLine = bufferedReader.readLine()) > -0.1 && (sCurrentLine = bufferedReader.readLine()) < 0){
                bufferedWriter1.write(0);
                bufferedWriter1.newLine();
            }else if((sCurrentLine = bufferedReader.readLine()) > 0 && (sCurrentLine = bufferedReader.readLine()) < 0.1){
                bufferedWriter1.write(0.1);
                bufferedWriter1.newLine();
            }else if((sCurrentLine = bufferedReader.readLine()) > 0.1);{
                bufferedWriter1.write(0.2);
                bufferedWriter1.newLine();
            }

            if((sCurrentLine = bufferedReader.readLine()) < -0.3){
                bufferedWriter2.write(-0.4);
                bufferedWriter2.newLine();
            }else if((sCurrentLine = bufferedReader.readLine()) > -0.3 && (sCurrentLine = bufferedReader.readLine()) < -0.25){
                bufferedWriter2.write(-0.3);
                bufferedWriter2.newLine();
            }else if((sCurrentLine = bufferedReader.readLine()) > -0.25 && (sCurrentLine = bufferedReader.readLine()) < -0.2){
                bufferedWriter2.write(-0.2);
                bufferedWriter2.newLine();
            }else if((sCurrentLine = bufferedReader.readLine()) < -0.2 && (sCurrentLine = bufferedReader.readLine()) > -0.15){
                bufferedWriter2.write(-0.1);
                bufferedWriter2.newLine();
            }else if((sCurrentLine = bufferedReader.readLine()) < -0.15 && (sCurrentLine = bufferedReader.readLine()) > -0.1){
                bufferedWriter2.write(-0.1);
                bufferedWriter2.newLine();
            }else if((sCurrentLine = bufferedReader.readLine()) < -0.1 && (sCurrentLine = bufferedReader.readLine()) > -0.05){
                bufferedWriter2.write(-0);
                bufferedWriter2.newLine();
            }else if((sCurrentLine = bufferedReader.readLine()) > -0.05 && (sCurrentLine = bufferedReader.readLine()) < 0){
                bufferedWriter2.write(0.1);
                bufferedWriter2.newLine();
            }else if((sCurrentLine = bufferedReader.readLine()) > 0 && (sCurrentLine = bufferedReader.readLine()) < 0.05){
                bufferedWriter2.write(0.2);
                bufferedWriter2.newLine();
            }else if((sCurrentLine = bufferedReader.readLine()) > 0.05 && (sCurrentLine = bufferedReader.readLine()) < 0.1){
                bufferedWriter2.write(0.3);
                bufferedWriter2.newLine();
            }else if((sCurrentLine = bufferedReader.readLine()) > 0.1);{
                bufferedWriter2.write(0.4);
                bufferedWriter2.newLine();
            }


            /*while(line != null){ 
            	System.out.println(line);
           		
        	}*/            
            bufferedWriter.close();

        }catch(FileNotFoundException ex) {
            System.out.println(
                "Unable to open file '" + 
                fileinputName + "'");                
        }
        catch(IOException ex) {
            System.out.println(
                "Error reading file '" 
                + fileoutputName + "'");                  
            // Or we could just do this: 
            // ex.printStackTrace();
        }


    }

    /*public void openFile(){
    	try {
    		xcanner = new Scanner (new File("New Text Document.txt"));
    	}
    	catch (Exception e){
    		System.out.println("Couldn't find the file");
    	}
    }
    public void readFile(){
    	while（xcanner.hasNext()){
			String a = x.next();
			String b = x.next();
			String c = x.next();

			System.out.printf("%s %s %s\n", a,b,c);
		}
    }

    public void closeFile(){
    	x.close();
    }*/
}
