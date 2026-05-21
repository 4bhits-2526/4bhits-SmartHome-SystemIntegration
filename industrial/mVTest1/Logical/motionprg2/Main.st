PROGRAM _INIT

	MpAxisBasic_0.Enable := TRUE;
	MC_BR_ProcessParID_AcpAx_Exe := TRUE;
    
	AxisParameters.Position := 100.0;
	AxisParameters.Velocity := 10.0;
	AxisParameters.Acceleration := 50.0;
	AxisParameters.Deceleration := 50.0;
	ParId719Value := 1;
	McAcpAxProcessParIDType_var.DataType 		:= mcACPAX_PARTYPE_UDINT;
	McAcpAxProcessParIDType_var.ParID 			:= mcACPPAR_ENCOD_CMD;
	McAcpAxProcessParIDType_var.VariableAddress := ADR(ParId719Value);
     
END_PROGRAM

PROGRAM _CYCLIC
    
	MpAxisBasic_0.MpLink     := ADR(gAxis_2);
	MpAxisBasic_0.Parameters := ADR(AxisParameters);
	MpAxisBasic_0();
     
	MC_BR_ProcessParID_AcpAx_0(Axis := ADR(gAxis_2), Execute := MC_BR_ProcessParID_AcpAx_Exe, DataAddress := ADR(McAcpAxProcessParIDType_var), NumberOfParIDs := 1, Mode := mcACPAX_PARID_SET);
	IF(MC_BR_ProcessParID_AcpAx_0.Error OR MC_BR_ProcessParID_AcpAx_0.Done) THEN
		MC_BR_ProcessParID_AcpAx_Exe 			:= FALSE;
	END_IF
	
	
IF((OPCUA_R2_Down <> 0) OR (mV_R2_Down <> 0)) THEN 
		OPCUA_R2_Down := 0;
		mV_R2_Down := 0;
		MC_MoveVelocity_0(Axis := ADR(gAxis_2), Execute := 1, Velocity := 30.0, Acceleration := 10.0, Deceleration := 10.0, Jerk := 0, Direction := mcPOSITIVE_DIR, BufferMode := mcABORTING);
END_IF

IF((OPCUA_R2_Up <> 0) OR (mV_R2_Up <> 0)) THEN 
		OPCUA_R2_Up := 0;
		mV_R2_Up := 0;
		MC_MoveVelocity_1(Axis := ADR(gAxis_2), Execute := 1, Velocity := 30.0, Acceleration := 10.0, Deceleration := 10.0, Jerk := 0, Direction := mcNEGATIVE_DIR, BufferMode := mcABORTING);
END_IF

	
END_PROGRAM