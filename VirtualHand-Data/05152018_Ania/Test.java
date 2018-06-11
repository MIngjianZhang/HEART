import java.io.*;
import java.util.*;

public class Test {

	private Scanner xcanner;
	
    public static void main(String [] args) {

        // The name of the file to open.
        String fileinputName = "New Text Document.txt";
        String fileoutputName = "CyberGloveData.txt";
        String line = "what the fuvckc";
        String[] temp = null;
        String CGData = "Testing";



        try{
            FileReader fileReader = new FileReader(fileinputName);

            BufferedReader bufferedReader = new BufferedReader(fileReader);
            // Assume default encoding.
            FileWriter fileWriter = new FileWriter(fileoutputName);

            // Always wrap FileWriter in BufferedWriter.
            BufferedWriter bufferedWriter = new BufferedWriter(fileWriter);
            
            line = bufferedReader.readLine();
            
            
            String sCurrentLine;

            while((sCurrentLine = bufferedReader.readLine()) != null){
            	
            	if(sCurrentLine.startsWith("5") == true){
                	temp = sCurrentLine.split(" ");
                	CGData = temp[2];
                	System.out.println(sCurrentLine);
                	bufferedWriter.write(CGData);
                	bufferedWriter.newLine();
            	}
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
