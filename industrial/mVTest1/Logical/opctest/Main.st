
PROGRAM _INIT
	(* Insert code here *)
	gSchweibsChange := gSchweibsWrite;
	
	myDO01 := myDO02 := myDO03 := myDO04 := myDO05 := myDO06 := myDO07 := myDO08 := myDO09 := myDO10 := myDO11 := myDO12 := 0;
	
	
	mySinValue := mySinValueIn := 0;
	
END_PROGRAM

PROGRAM _CYCLIC
	(* Insert code here *)
	gSchweibsChange := gSchweibsChange + 1;
	
	mySinValueIn := mySinValueIn + 0.001;
	mySinValue := SIN(mySinValueIn);
	
	
	
	
END_PROGRAM

PROGRAM _EXIT
	(* Insert code here *)
	 
END_PROGRAM

