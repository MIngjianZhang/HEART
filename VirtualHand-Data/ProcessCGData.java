import java.io.*;
import java.util.*;

public class ProcessCGData{
	private Scanner x;

	public static void main(String[] args){
		openFile();
		readFile();
		closeFile();
		
	}

	public void openFile(){
		try{
			x = new Scanner(new File("Chinese.txt"));
		}
		catch (Exception e){
			System.out.println("could not find file");
		}
	}

	public void readFile(){
		while(x.hasNext()){
			String a = x.next();
			String b = x.next();
			String c = x.next();

			System.out.printf("%s %s %s", a,b,c);
		}
	}

	public void closeFile(){
		x.close();
	}

}