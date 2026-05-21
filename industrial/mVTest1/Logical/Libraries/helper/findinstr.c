
#include <bur/plctypes.h>
#include <string.h>
#ifdef __cplusplus
	extern "C"
	{
#endif
	#include "helper.h"
#ifdef __cplusplus
	};
#endif
/* TODO: Add your comment here */
plcbit findinstr(plcstring* String1,plcstring* String2)
{
	/*TODO: Add your code here*/
	char * mysubstring = strstr(String1, String2);
	if (mysubstring != NULL) {
		return 1;
	}
	else
	{
		return 0;
	}
}
