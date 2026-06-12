
PROGRAM _INIT
	(* Insert code here *)
	ST_name(0,ADR(myTaskName),0);
	RTInfo_0(enable := TRUE);
	IF(RoomName = '') THEN
		IF (myTaskName = 'room1') THEN
			RoomName := 'Kitchen';
		END_IF
		IF (myTaskName = 'room2') THEN
			RoomName := 'Living';
		END_IF
		IF (myTaskName = 'room3') THEN
			RoomName := 'Bedroom';
		END_IF
	END_IF
	
END_PROGRAM

PROGRAM _CYCLIC
	(* Insert code here *)
	
	(*raising edge*)
	r_trig_HW(CLK := SwitchValueHW);
	r_trig_T(CLK := SwitchValueT);
	r_trig_GL(CLK := SwitchValueGL);
	r_trig_W(CLK := SwitchValueW);
	
	(*all inputs*)
	SwitchValueTrig := r_trig_HW.Q OR r_trig_T.Q OR r_trig_GL.Q OR r_trig_W.Q;
	
	
	
	IF(r_trig_HW.Q) THEN
		Manipulator := 'Hardware';
	END_IF
	IF(r_trig_T.Q) THEN
		Manipulator := 'Tablett';
	END_IF
	IF(r_trig_GL.Q) THEN
		Manipulator := 'Glasses';
	END_IF
	IF(r_trig_W.Q) THEN
		Manipulator := 'Windows';
	END_IF
	
	
	(*control lamp*)
	IF (SwitchValueTrig = TRUE) THEN
		Lampe := NOT(Lampe);
		LampeSwitchCnt := LampeSwitchCnt + 1;
		gSendMessage := myTaskName;
		brsstrcat(ADR(gSendMessage), ADR('/Lampe='));
		IF (Lampe) THEN
			brsstrcat(ADR(gSendMessage), ADR('TRUE$n'));
		ELSE
			brsstrcat(ADR(gSendMessage), ADR('FALSE$n'));
		END_IF
		gDataSend := TRUE;
	END_IF
	(*tcp communication*)
	IF (SendValue = TRUE) THEN
		brsstrcat(ADR(gSendMessage), ADR(myTaskName));
		brsstrcat(ADR(gSendMessage), ADR('/Lampe='));
		IF (Lampe) THEN
			brsstrcat(ADR(gSendMessage), ADR('TRUE$n'));
		ELSE
			brsstrcat(ADR(gSendMessage), ADR('FALSE$n'));
		END_IF
		brsstrcat(ADR(gSendMessage), ADR(myTaskName));
		brsstrcat(ADR(gSendMessage), ADR('/LampeRT='));
		tmpString := UDINT_TO_STRING(LampeRT);
		brsstrcat(ADR(gSendMessage), ADR(tmpString));
		brsstrcat(ADR(gSendMessage), ADR('$n'));
		brsstrcat(ADR(gSendMessage), ADR(myTaskName));
		brsstrcat(ADR(gSendMessage), ADR('/LampeSwCnt='));
		tmpString := UDINT_TO_STRING(LampeSwitchCnt);
		brsstrcat(ADR(gSendMessage), ADR(tmpString));
		brsstrcat(ADR(gSendMessage), ADR('$n'));

		gDataSend := TRUE;
		SendValue := FALSE;
	END_IF
	IF(Lampe) THEN
		LampeRuntimeus := LampeRuntimeus + RTInfo_0.cycle_time;
		IF(LampeRuntimeus >= 1000000) THEN
			LampeRT := LampeRT + 1; // Sekunden
			LampeRuntimeus := LampeRuntimeus - 1000000;
		END_IF
	END_IF
	IF (LampeNew) THEN
		LampeNew := FALSE;
		LampeRuntimeus := 0;
		LampeRT := 0;
	END_IF
	
	
	 
END_PROGRAM

PROGRAM _EXIT
	(* Insert code here *)
	 
END_PROGRAM

