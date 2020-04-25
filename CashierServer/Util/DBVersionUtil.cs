using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierServer.Util
{
    class DBVersionUtil
    {

        public static string procGain(string proc)
        {
            switch (proc)
            {
                case "addOnlineNew":
                    return procAddOnlineNew();
                case "dutyDataSaveNew":
                    return procDutyDataSaveNew();
                case "updateOnlineByCrossArea":
                    return procUpdateOnlineByCrossArea();
                case "clearOnlineHistory":
                    return procClearOnlineHistory();
                default:
                    return "";
            }
        }

        public static string procAddOnlineNew()
        {
            string proc = @"CREATE DEFINER=`admin`@`'%'` PROCEDURE `addOnlineNew`(IN memberID_ bigint(20) unsigned,
    IN memberType_ tinyint,
    IN onlineRoomID_ INT,
    IN ifRoomOwner_ TINYINT,
    IN startUser_ INT,
    IN gid_ INT,
    IN payWay_ TINYINT,
    IN tariffConfigID_ INT,
    IN tariffDataVersion_ INT,
    IN authoriseUser_ INT,
	IN tariffType_ TINYINT,
    IN areaID_ INT,
    IN idseq bigint unsigned,
	IN ruleID_ INT,
    OUT result INT)
    SQL SECURITY INVOKER
    COMMENT '开卡记录，新版本服务器添加规则ID写入'
lab:BEGIN
    declare lseq varchar(7);
	declare onlineRoomIDNew_ bigint unsigned;
    declare nowtime varchar(30);
    declare no_rows bigint;
    declare na_rows int;
    declare areaName_ varchar(30);
    declare deposit_ decimal(8,2);
    declare nm_rows int;
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION set result=0;    

    select count(*) into nm_rows from netbar_member where memberID = memberID_ and gid = gid_;
	if nm_rows = 0 then
		set result = 1403;
		leave lab;
	end if;

    if areaID_ != 0 and areaID_ is not null then 
		select count(*) into na_rows from netbar_area where areaID = areaID_ and gid = gid_;
		if na_rows = 0 then
			set result = 14033;
			leave lab;
		else
        select areaName into areaName_ from netbar_area where areaID = areaID_ and gid = gid_;
		end if;
    end if; 

	if ruleID_ is null then
		set ruleID_ = 0;
	end if;

    start transaction;
    if tariffType_ = 1  then
	
    insert into netbar_online
    (memberID,memberType,onlineRoomID,ifRoomOwner,state,gid,dataVersion,theDate,startUser,startCardTime,tariffConfigID,payWay,areaID,tariffType,authoriseUser,ruleID)
	values
    (memberID_,memberType_,onlineRoomID_,ifRoomOwner_,3,gid_,1,CURDATE(),startUser_,now(),tariffConfigID_,payWay_,areaID_,tariffType_,authoriseUser_,ruleID_);
	
    elseif tariffType_ = 4  and areaID_!=0 then    
	
    set onlineRoomIDNew_ = idseq;
    select now() into nowtime;
	
    insert into netbar_online
    (memberID,memberType,onlineRoomID,ifRoomOwner,state,gid,dataVersion,theDate,startUser,startCardTime,tariffConfigID,payWay,areaID,tariffType,authoriseUser,areaName,ruleID)
	values
    (memberID_,memberType_,onlineRoomIDNew_,ifRoomOwner_,3,gid_,1,CURDATE(),startUser_,nowtime,tariffConfigID_,payWay_,areaID_,tariffType_,authoriseUser_,areaName_,ruleID_);

    insert into netbar_online_room
    (onlineRoomID,areaID,areaName,state,tariffType,payWay,startTime,startUser,gid,dataVersion,ruleID)
    values
    (onlineRoomIDNew_,areaID_,areaName_,1,tariffType_,payWay_,nowtime,startUser_,gid_,1,ruleID_);

    elseif (tariffType_ = 5 or tariffType_ = 6) and areaID_!=0 then    

    set onlineRoomIDNew_ = idseq;
    select now() into nowtime;
	
    insert into netbar_online
    (memberID,memberType,onlineRoomID,ifRoomOwner,state,gid,dataVersion,theDate,startUser,startCardTime,tariffConfigID,payWay,areaID,tariffType,authoriseUser,areaName,actTime,ruleID)
	values
    (memberID_,memberType_,onlineRoomIDNew_,ifRoomOwner_,3,gid_,1,CURDATE(),startUser_,nowtime,tariffConfigID_,payWay_,areaID_,tariffType_,authoriseUser_,areaName_,nowtime,ruleID_);

    insert into netbar_online_room
    (onlineRoomID,areaID,areaName,state,tariffType,payWay,startTime,startUser,gid,dataVersion,endTime,ruleID)
    values
    (onlineRoomIDNew_,areaID_,areaName_,1,tariffType_,payWay_,nowtime,startUser_,gid_,1,nowtime,ruleID_);

    elseif (tariffType_ = 2 or tariffType_ = 3) and areaID_ !=0  then

	insert into netbar_online
	(memberID,memberType,onlineRoomID,ifRoomOwner,state,gid,dataVersion,theDate,startUser,startCardTime,tariffConfigID,payWay,areaID,tariffType,authoriseUser,areaName,actTime,ruleID)
	values
	(memberID_,memberType_,onlineRoomID_,ifRoomOwner_,3,gid_,1,CURDATE(),startUser_,now(),tariffConfigID_,payWay_,areaID_,tariffType_,authoriseUser_,areaName_,now(),ruleID_);

    else
		set result = 14031;
    end if;

    select count(*) into no_rows from netbar_online where memberID = memberID_ and offLineTime is null and gid = gid_;
    if no_rows > 1 then
		set result = 14032;
    end if; 

    select deposit into deposit_ from netbar_member_account where memberID = memberID_ and gid = gid_;
    if deposit_ > 0 then
    update netbar_online set depositState = 0,dataVersion = dataVersion + 1 where memberID = memberID_ and offLineTime is null and gid = gid_;
    end if;

	if result is null then
	  commit;
      set result = 1;
	else
	  rollback;
    end if;    
END ;;";
            return proc;
        }

        public static string procDutyDataSaveNew()
        {
            string proc = @"CREATE DEFINER=`admin`@`'%'` PROCEDURE `dutyDataSaveNew`(IN cupScrapArray_ varchar(1000),
    IN dutyDate_ datetime,
	IN beginDate_ datetime,
	IN endDate_  datetime,
	IN revenueArray_ varchar(1000),
	IN payWayArray_ varchar(1000),
	IN problemArray_  varchar(1000),
	IN currentnetBarUserID_ int ,
	IN nextnetBarUserID_ int ,
	IN shiftID_ int,
    IN nextShiftID_  int,
	IN dutyID_ INT,
	IN gid_ int,
	OUT result int)
    SQL SECURITY INVOKER
lab:BEGIN
	declare strlen int ;
	declare last_index int DEFAULT 0;
	declare cur_index int DEFAULT 1;
	declare len int;
	DECLARE submit_time DATETIME;
	declare cupScrapStr varchar(100);
	declare cupID_ int;
	declare theScrapNum_ int;
	declare problemStr varchar(200);
	declare problemID_ int;
	declare description_ varchar(100);
	declare state_ tinyint(1);
	declare creator_ int;
	declare actor_ int;
	declare remark_ varchar(100);
	declare currentSum_ decimal(10,2);
	DECLARE cashReserveSum_ decimal(10,2);
	DECLARE lastReserve_ DECIMAL(10,2);
	DECLARE rerserv_cash_ DECIMAL(10,2);
	DECLARE memberCharge_ decimal(10,2);
	DECLARE tmpFee_ decimal(10,2);
	DECLARE tmpDeposit_ decimal(10,2);
	DECLARE goodsFee_ decimal(10,2);
	DECLARE waterBarFee_ decimal(10,2);
	DECLARE otherIn_ decimal(10,2);
	DECLARE otherOut_ decimal(10,2);
	DECLARE goodsOutFee_ decimal(6,2);
	DECLARE netOutFee_ decimal(6,2);
	DECLARE cashFee_ decimal(10,2);
	DECLARE bankFee_ decimal(10,2);
	DECLARE alipayFee_ decimal(10,2);
	DECLARE weixinFee_ decimal(10,2);
    declare acount_ decimal(10,2);
    declare couponDeductionSum_ decimal(10,2);
	declare onlineFee_ decimal(10,2);
	declare shiftIDN_ int;
    declare  machineNum_  int;
    declare totalInFee_ decimal(10,2);
	declare chargeInFee_ decimal(10,2);
	declare onlineInFee_ decimal(10,2);
	declare goodsInFee_ decimal(10,2);
	declare otherInFee_ decimal(10,2);
	declare totalConsume_ decimal(10,2);
	declare memberOnlineConsume_ decimal(10,2);
	declare tmpOnlineConsume_ decimal(10,2);
	declare goodsConsume_ decimal(10,2);
	declare waterConsume_ decimal(10,2);
	declare goodsConsumeT_ decimal(10,2);
	declare goodsConsumeC_ decimal(10,2);
	declare waterConsumeT_ decimal(10,2);
	declare waterConsumeC_ decimal(10,2);
	declare onlineTimes_ int;
	declare onlineTimesp_ int;
	declare onlineLongs_ decimal(10,2);
	declare oldeMember_ int;
	declare newsMember_ int;
	declare tmpMember_ int;
	declare attendence_ decimal(10,4);
	declare theSum_ decimal(10,2);
	declare goodsNum_ int;
	declare watersNum_ int;
	declare adwardFeeSum_ decimal(10,2);
    declare shiftName_ varchar(30);
    declare currentName_ varchar(30);
    declare nextShiftName_ varchar(30);
    declare nextName_ varchar(30);
    declare n int ;
	DECLARE CONTINUE HANDLER FOR SQLEXCEPTION set result=0;

	select count(*) into n from netbar_duty where gid = gid_ and  dutyDate=  dutyDate_ and  dutyBeginTime = beginDate_  and  dutyEndTime = endDate_;
	if n >0 then
		set result = 10635;
		leave lab;
	end if;
	
	select memberName into  currentName_ from netbar_user where netbarUserID = currentnetBarUserID_ and gid = gid_;
	select memberName into  nextName_ from netbar_user where netbarUserID = nextnetBarUserID_ and gid = gid_;
	select shiftName into shiftName_ from netbar_shift where shiftId = shiftID_  and gid = gid_;
	select shiftName into nextShiftName_ from netbar_shift where shiftId = nextShiftID_  and gid = gid_;
	set shiftIDN_ = shiftID_;
	set submit_time = now();

	if beginDate_ is null || beginDate_ = '' then
		select date(a) into beginDate_ from (
		select startCardTime as a from netbar_online where gid = gid_ 
		union all 
		select createTime as a from netbar_goods_order where gid = gid_
		union all
		select rechargeDate as a from netbar_recharge_order where gid = gid_ ) as b order by a asc limit 1;
	end if;

	start transaction;
	if cupScrapArray_ != '' || cupScrapArray_ is not null then -- 报废列表处理
		set last_index = 0;
		set cur_index = 1;
		set strlen = length(cupScrapArray_);
		WHILE(cur_index<=strlen) DO
		begin
			if substring(cupScrapArray_ from cur_index for 1)=',' or cur_index=strlen then
				set len=cur_index-last_index-1;
					if cur_index=strlen then
						set len=len+1;
					end if;
		set cupScrapStr = substring(cupScrapArray_ from (last_index+1) for len);
		set cupID_  =  cast(SUBSTRING_INDEX(cupScrapStr,':',1) as SIGNED);
		set theScrapNum_  = cast(SUBSTRING_INDEX(cupScrapStr,':',-1) as SIGNED);

		insert into netbar_duty_cup_scrap (cupID,dutyID,theNum,dataVersion,gid,shiftName,cupName) values (cupID_,dutyID_,theScrapNum_,1,gid_,shiftName_,(select cupName from netbar_cup where gid = gid_ and cupID = cupID_));
		update netbar_cup set theNum = theNum - theScrapNum_ where cupID = cupID_ and gid = gid_;
			set last_index=cur_index;
			end if;
		set cur_index=cur_index+1;
		END;
		end while;
	end if;

	set cashReserveSum_ = cast(SUBSTRING_INDEX(revenueArray_,':',1) as DECIMAL(10,2));
	set lastReserve_= cast(SUBSTRING_INDEX(SUBSTRING_INDEX(revenueArray_,':',2),':',-1) as DECIMAL(10,2));
	set rerserv_cash_ = cast(SUBSTRING_INDEX(SUBSTRING_INDEX(revenueArray_,':',3),':',-1) as DECIMAL(10,2));
	set memberCharge_ = cast(SUBSTRING_INDEX(SUBSTRING_INDEX(revenueArray_,':',4),':',-1) as DECIMAL(10,2));
	set tmpFee_ = cast(SUBSTRING_INDEX(SUBSTRING_INDEX(revenueArray_,':',5),':',-1) as DECIMAL(10,2));
	set tmpDeposit_ = cast(SUBSTRING_INDEX(SUBSTRING_INDEX(revenueArray_,':',6),':',-1) as DECIMAL(10,2));
	set goodsFee_ = cast(SUBSTRING_INDEX(SUBSTRING_INDEX(revenueArray_,':',7),':',-1) as DECIMAL(10,2));
	set waterBarFee_ = cast(SUBSTRING_INDEX(SUBSTRING_INDEX(revenueArray_,':',8),':',-1) as DECIMAL(10,2));
	set otherIn_ = cast(SUBSTRING_INDEX(SUBSTRING_INDEX(revenueArray_,':',9),':',-1) as DECIMAL(10,2));
	set otherOut_ = cast(SUBSTRING_INDEX(SUBSTRING_INDEX(revenueArray_,':',10),':',-1) as DECIMAL(10,2));
	set goodsOutFee_ = cast(SUBSTRING_INDEX(SUBSTRING_INDEX(revenueArray_,':',11),':',-1) as DECIMAL(10,2));
	set netOutFee_ = cast(SUBSTRING_INDEX(SUBSTRING_INDEX(revenueArray_,':',12),':',-1) as DECIMAL(10,2));
	set currentSum_ = lastReserve_+memberCharge_+tmpFee_+tmpDeposit_+goodsFee_+waterBarFee_+otherIn_ - otherOut_;

	update netbar_duty_extra_fee set dutyID = dutyID_,dutyDate = dutyDate_, dutyBeginTime = beginDate_, dutyEndTime = endDate_ where createDate > beginDate_ and createDate <= endDate_;
	if problemArray_ != '' || problemArray_ is not null then -- 问题列表处理
		set last_index = 0;
		set cur_index = 1;
		set strlen = length(problemArray_);

		WHILE(cur_index<=strlen) DO
			begin
			if substring(problemArray_ from cur_index for 1)=',' or cur_index=strlen then
					set len=cur_index-last_index-1;
						if cur_index=strlen then
							set len=len+1;
						end if;
			set problemStr = substring(problemArray_ from (last_index+1) for len);
			set problemID_  =  cast(SUBSTRING_INDEX(problemStr,':',1) as SIGNED);
			set state_= cast(SUBSTRING_INDEX(SUBSTRING_INDEX(problemStr,':',3),':',-1) as SIGNED);
			if problemID_ = -1 then
				set description_= SUBSTRING_INDEX(SUBSTRING_INDEX(problemStr,':',2),':',-1);
				set creator_= cast(SUBSTRING_INDEX(SUBSTRING_INDEX(problemStr,':',4),':',-1) as SIGNED);
				insert into netbar_duty_problem(dutyID,description,state,creator,createDate,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime) values
				(dutyID_,description_,state_,creator_,now(),gid_,shiftIDN_,dutyDate_,beginDate_,endDate_);
			end if;
			if problemID_ is not null && problemID_ != -1 then
				set actor_= cast(SUBSTRING_INDEX(SUBSTRING_INDEX(problemStr,':',5),':',-1) as SIGNED);
				set remark_ = SUBSTRING_INDEX(SUBSTRING_INDEX(problemStr,':',6),':',-1);
				update netbar_duty_problem set state = state_ ,actor = actor_ ,remark = remark_ where problemID = problemID_;
			end if;
			set last_index=cur_index;
			end if;

			set cur_index=cur_index+1;

			END;

		end while;

	end if;

    select count(*)  into machineNum_ from netbar_machine where state = 1 and gid = gid_;
	select IFNULL(sum(rechargeFee),0) into chargeInFee_ from netbar_recharge_order where gid = gid_ and state = 3 and rechargeDate > beginDate_ and rechargeDate <= endDate_ and rechargeSource != 5 and memberID not in (select  memberID from  netbar_member where memberType = 1 and gid = gid_);
	select IFNULL(sum(onlineFee - couponDeduction),0) into onlineInFee_ from netbar_online where gid = gid_ and state = 2 and offLineTime >beginDate_ and offLineTime <= endDate_ and payWay != 5;
	select IFNULL(sum(paySum),0) into goodsInFee_ from netbar_goods_order where gid = gid_ and state = 4 and actTime >beginDate_ and actTime <= endDate_  and payWay != 5;
	select IFNULL(sum(fee),0) into otherInFee_ from netbar_duty_extra_fee WHERE gid = gid_ and extraFeeType = 1 and extra = 4 and dutyID = dutyID_;
	select IFNULL(sum(onlineFee - couponDeduction),0) into memberOnlineConsume_ from
	(select onlineFee , couponDeduction from netbar_online where gid = gid_ and state = 2 and offLineTime > beginDate_ and offLineTime <=  endDate_ and memberType != 1
	union all
	select onlineFee , couponDeduction from netbar_online_history where gid = gid_ and state = 2 and offLineTime > beginDate_ and offLineTime <=  endDate_ and memberType != 1) as a;
	select IFNULL(sum(onlineFee - couponDeduction),0) into tmpOnlineConsume_ from
	(select onlineFee,couponDeduction from netbar_online where gid = gid_ and state = 2 and offLineTime > beginDate_ and offLineTime <=  endDate_  and  memberType = 1
	union all
	select onlineFee, couponDeduction from netbar_online_history where gid = gid_ and state = 2 and offLineTime > beginDate_ and offLineTime <=  endDate_  and  memberType = 1) as a;
	select IFNULL(sum(payFee*theNum),0) into goodsConsumeT_ from netbar_goods_order_detail where gid = gid_ and goodsOrderID in(select goodsOrderID from netbar_goods_order where gid = gid_ and state = 4 and actTime > beginDate_ and actTime <= endDate_ ) and cupID = -1;
	select IFNULL(sum(payFee*theNum),0) into waterConsumeT_ from netbar_goods_order_detail where gid = gid_ and goodsOrderID in(select goodsOrderID from netbar_goods_order where gid = gid_ and state = 4 and actTime > beginDate_ and actTime <= endDate_ ) and cupID != -1;
	select IFNULL(sum(deductionFee),0) into goodsConsumeC_ from netbar_coupon_use where goodsOrderID in(select goodsOrderID from netbar_goods_order where gid = gid_ and state = 4 and actTime > beginDate_ and actTime <= endDate_ ) and state = 1 and gid = gid_ and deductionCupID = -1;
	select IFNULL(sum(deductionFee),0) into waterConsumeC_ from netbar_coupon_use where goodsOrderID in(select goodsOrderID from netbar_goods_order where gid = gid_ and state = 4 and actTime > beginDate_ and actTime <= endDate_ ) and state = 1 and gid = gid_ and deductionCupID != -1 and deductionCupID is not null;
	set goodsConsume_ = goodsConsumeT_ - goodsConsumeC_;
	set waterConsume_ = waterConsumeT_ - waterConsumeC_;
	set totalConsume_ = memberOnlineConsume_ + tmpOnlineConsume_ + goodsConsume_ + waterConsume_;
	set totalInFee_ = chargeInFee_ +  goodsInFee_ + otherInFee_+tmpOnlineConsume_;
	select IFNULL(sum(theNum),0) into goodsNum_ from netbar_goods_order_detail where gid = gid_ and goodsOrderID in(select goodsOrderID from netbar_goods_order where gid = gid_ and  state = 4 and actTime > beginDate_ and actTime <= endDate_ ) and cupID = -1;
	select IFNULL(sum(theNum),0) into watersNum_ from netbar_goods_order_detail where gid = gid_ and goodsOrderID in(select goodsOrderID from netbar_goods_order where gid = gid_ and  state = 4 and actTime > beginDate_ and actTime <= endDate_ ) and cupID != -1;
	select sum(b) into onlineTimes_ from
	(select count(*) as b from netbar_online where gid = gid_ and offLineTime > beginDate_ and offLineTime <= endDate_
	union all
	select count(*) as b from netbar_online_history where gid = gid_ and offLineTime > beginDate_ and offLineTime <= endDate_ ) as a ;
	select count(*) into onlineTimesp_ from
	(select memberID  from netbar_online where gid = gid_ and offLineTime > beginDate_ and offLineTime <= endDate_
	union 
	select memberID  from netbar_online_history where gid = gid_ and offLineTime > beginDate_ and offLineTime <= endDate_   group by memberID ) as a;
	select sum(b) into onlineLongs_ from
	(select internetTime as b from netbar_online where gid = gid_ and offLineTime > beginDate_ and offLineTime <= endDate_ and state =2
	union all
	select internetTime as b from netbar_online_history where gid = gid_ and offLineTime > beginDate_ and offLineTime <= endDate_ and state =2 ) as a ;
	select sum(b) into oldeMember_ from
	(select count(*) as b from netbar_online where gid = gid_ and offLineTime > beginDate_ and offLineTime <= endDate_ and memberID in(select memberID from netbar_member where  gid = gid_  and state = 1 and createDate <=beginDate_ ) and memberType != 1
	union all
	select count(*)  as b  from netbar_online_history where gid = gid_  and offLineTime > beginDate_ and offLineTime <=  endDate_  and memberID in(select  memberID from netbar_member where  gid = gid_  and state = 1 and createDate <=beginDate_ ) and memberType != 1) as a;
	select count(*) into newsMember_ from netbar_member where gid = gid_ and state = 1 and createDate > beginDate_ and createDate <= endDate_ and memberType != 1;
	select count(*) into tmpMember_ from netbar_member where gid = gid_ and state = 1 and createDate > beginDate_ and createDate <= endDate_ and memberType = 1;
	
	select format((IFNULL(sum(theTime)/
	((select count(*) from netbar_machine where gid = gid_ and netbar_machine.state = 1 )*(select timestampdiff (SECOND,beginDate_,endDate_))),0)),2) into attendence_
	from
	(select sum(timestampdiff (SECOND,onlineStartTime,offLineTime)) as theTime
	from netbar_online where onlineStartTime > beginDate_ and offLineTime <= endDate_ and areaID != 0  and gid = gid_
	union all
	select sum(timestampdiff (SECOND,onlineStartTime,endDate_)) as theTime
	from netbar_online where onlineStartTime > beginDate_ and onlineStartTime <= endDate_ and (offLineTime > endDate_ or offLineTime is null) and areaID != 0 and gid = gid_
	union all
	select sum(timestampdiff (SECOND,beginDate_,offLineTime)) as theTime
	from netbar_online where onlineStartTime <= beginDate_ and offLineTime <= endDate_ and offLineTime > beginDate_ and areaID != 0 and gid = gid_
	union all
	select sum(timestampdiff (SECOND,beginDate_,endDate_)) as theTime
	from netbar_online where onlineStartTime <= beginDate_ and (offLineTime > endDate_ or offLineTime is null) and areaID != 0 and gid = gid_
	union all
	select sum(timestampdiff (SECOND,onlineStartTime,offLineTime)) as theTime
	from netbar_online_history where onlineStartTime > beginDate_ and onlineStartTime <= endDate_ and areaID != 0 and gid = gid_
	union all
	select sum(timestampdiff (SECOND,onlineStartTime,endDate_)) as theTime
	from netbar_online_history where onlineStartTime > beginDate_ and onlineStartTime <= endDate_ and (offLineTime > endDate_ or offLineTime is null) and areaID != 0 and gid = gid_
	union all
	select sum(timestampdiff (SECOND,beginDate_,offLineTime)) as theTime
	from netbar_online_history where onlineStartTime <= beginDate_ and offLineTime <= endDate_ and offLineTime > beginDate_ and areaID != 0 and gid = gid_
	union all
	select sum(timestampdiff (SECOND,beginDate_,endDate_)) as theTime
	from netbar_online_history where onlineStartTime <= beginDate_ and (offLineTime > endDate_ or offLineTime is null) and areaID != 0 and gid = gid_
	) as a;

	select IFNULL(sum(paySum),0) into theSum_ from netbar_goods_order where gid = gid_ and  state = 4 and actTime > beginDate_ and actTime <= endDate_ ;
	select sum(adwardFee) into adwardFeeSum_ from  netbar_recharge_order where gid = gid_ and state = 3 and rechargeDate > beginDate_ and rechargeDate  <= endDate_ ;
    set cashFee_ = cast(SUBSTRING_INDEX(payWayArray_,':',1) as DECIMAL(10,2));
	set bankFee_= cast(SUBSTRING_INDEX(SUBSTRING_INDEX(payWayArray_,':',2),':',-1) as DECIMAL(10,2));
	set alipayFee_= cast(SUBSTRING_INDEX(SUBSTRING_INDEX(payWayArray_,':',3),':',-1) as DECIMAL(10,2));
	set weixinFee_= cast(SUBSTRING_INDEX(SUBSTRING_INDEX(payWayArray_,':',4),':',-1) as DECIMAL(10,2));
	set acount_= cast(SUBSTRING_INDEX(SUBSTRING_INDEX(payWayArray_,':',5),':',-1) as DECIMAL(10,2));
    set couponDeductionSum_ = cast(SUBSTRING_INDEX(SUBSTRING_INDEX(payWayArray_,':',6),':',-1) as DECIMAL(10,2));
	
	insert into netbar_duty
	(dutyID,shiftID,currentCash,currentSum,currentDeliver,currentReserve,dutyBeginTime,dutyEndTime,state,remark,dataVersion,gid,generateFrom,currentnetBarUserID,nextnetBarUserID,submitTime,
	totalIncome,totalConsume,totalAttendance,goodsTotalIncome,newMemberNum,turnOverRatio,onlineTimes,onlineMembers,internetTimes,adwardTotal,couponDeduction,dutyDate,nextShiftID,shiftName,currentName,nextShiftName,nextName)
	values
	(dutyID_,shiftIDN_,cashReserveSum_ + lastReserve_,currentSum_,cashReserveSum_-rerserv_cash_-tmpDeposit_+lastReserve_,rerserv_cash_,beginDate_,endDate_,1,'',1,gid_,2,currentnetBarUserID_,nextnetBarUserID_,submit_time,
	totalInFee_,totalConsume_,attendence_,theSum_,newsMember_,format((onlineTimes_/machineNum_),2),onlineTimes_,onlineTimesp_,onlineLongs_ ,adwardFeeSum_,couponDeductionSum_,dutyDate_,nextShiftID_,shiftName_,currentName_,nextShiftName_,
	nextName_);

	insert into netbar_duty_income
	(dutyID,incomeType,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,incomeTypeName)
	values
	(dutyID_,1,chargeInFee_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 13 and parasID = 1));

	insert into netbar_duty_income
	(dutyID,incomeType,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,incomeTypeName)
	values
	(dutyID_,2,onlineInFee_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 13 and parasID = 2));

	insert into netbar_duty_income
	(dutyID,incomeType,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,incomeTypeName)
	values
	(dutyID_,3,goodsInFee_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 13 and parasID = 3));

	insert into netbar_duty_income
	(dutyID,incomeType,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,incomeTypeName)
	values
	(dutyID_,4,otherInFee_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 13 and parasID = 4));

	insert into netbar_duty_consume
	(dutyID,revenueType,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,revenueTypeName)
	values
	(dutyID_,1,memberOnlineConsume_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 14 and parasID = 1));

	insert into netbar_duty_consume
	(dutyID,revenueType,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,revenueTypeName)
	values
	(dutyID_,2,tmpOnlineConsume_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 14 and parasID = 2));

	insert into netbar_duty_consume
	(dutyID,revenueType,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,revenueTypeName)
	values
	(dutyID_,3,goodsConsume_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 14 and parasID = 3));

	insert into netbar_duty_consume
	(dutyID,revenueType,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,revenueTypeName)
	values
	(dutyID_,4,waterConsume_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 14 and parasID = 4));

	insert into netbar_duty_area (areaID,areaName,fee,attendance,dataVersion,gid,dutyID,turnOverRatio,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,theOnlineTime,theTimes)
	select
	a.areaID,
	(select areaName from netbar_area where areaID = a.areaID and gid = gid_) as areaName,
	ifnull(sum(a.fee),0),
	format((IFNULL(sum(a.theTime)/((select count(*) from netbar_machine where gid = gid_ and netbar_machine.state = 1 and netbar_machine.areaID = a.areaID)*(select  timestampdiff (SECOND,beginDate_,endDate_))),0)),2) as areaAttendence,
	1,
	gid_,
	dutyID_,
	format((sum(a.theTimes)/(select count(*) from netbar_machine where state = 1 and gid = gid_ and areaID = a.areaID)),2),
	shiftIDN_,
	dutyDate_,beginDate_,endDate_,shiftName_,sum(theTime) as theOnlineTime,sum(theTimes) as theTimes
	from
	(select areaID,sum(timestampdiff (SECOND,onlineStartTime,offLineTime)) as theTime,sum(onlineFee-couponDeduction) as fee,count(*)  as theTimes
	from netbar_online where onlineStartTime > beginDate_ and offLineTime <= endDate_ and areaID != 0 and gid = gid_ group by areaID
	union all
	select areaID,sum(timestampdiff (SECOND,onlineStartTime,endDate_)) as theTime,sum(onlineFee-couponDeduction) as fee,count(*)  as theTimes
	from netbar_online where onlineStartTime > beginDate_ and onlineStartTime <= endDate_ and (offLineTime > endDate_ or offLineTime is null) and areaID != 0  and gid = gid_ group by areaID
	union all
	select areaID,sum(timestampdiff (SECOND,beginDate_,offLineTime)) as theTime,sum(onlineFee-couponDeduction) as fee,count(*)  as theTimes
	from netbar_online where onlineStartTime <= beginDate_ and offLineTime <= endDate_ and offLineTime > beginDate_ and areaID != 0 and gid = gid_ group by areaID
	union all
	select areaID,sum(timestampdiff (SECOND,beginDate_,endDate_)) as theTime,sum(onlineFee-couponDeduction) as fee,count(*)  as theTimes
	from netbar_online where onlineStartTime <= beginDate_ and (offLineTime > endDate_ or offLineTime is null) and areaID != 0 and gid = gid_ group by areaID
	union all
	select areaID,sum(timestampdiff (SECOND,onlineStartTime,offLineTime)) as theTime,sum(onlineFee-couponDeduction) as fee,count(*)  as theTimes
	from netbar_online_history where onlineStartTime > beginDate_ and offLineTime <= endDate_ and areaID != 0 and gid = gid_ group by areaID
	union all
	select areaID,sum(timestampdiff (SECOND,onlineStartTime,endDate_)) as theTime,sum(onlineFee-couponDeduction) as fee,count(*)  as theTimes
	from netbar_online_history where onlineStartTime > beginDate_ and onlineStartTime <= endDate_ and (offLineTime > endDate_ or offLineTime is null) and areaID != 0 and gid = gid_ group by areaID
	union all
	select areaID,sum(timestampdiff (SECOND,beginDate_,offLineTime)) as theTime,sum(onlineFee-couponDeduction) as fee,count(*)  as theTimes
	from netbar_online_history where onlineStartTime <= beginDate_ and offLineTime <= endDate_ and offLineTime > beginDate_ and areaID != 0  and gid = gid_ group by areaID
	union all
	select areaID,sum(timestampdiff (SECOND,beginDate_,endDate_)) as theTime,sum(onlineFee-couponDeduction) as fee,count(*)  as theTimes
	from netbar_online_history where onlineStartTime <= beginDate_ and (offLineTime > endDate_ or offLineTime is null) and areaID != 0 and gid = gid_ group by areaID
	) as a group by a.areaID;

	insert into netbar_duty_member_online
	(dutyID,memberTag,theNum,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,memberTagName)
	values
	(dutyID_,1,oldeMember_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 15 and parasID = 1));

	insert into netbar_duty_member_online
	(dutyID,memberTag,theNum,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,memberTagName)
	values
	(dutyID_,2,newsMember_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 15 and parasID = 2));
	
	insert into netbar_duty_member_online
	(dutyID,memberTag,theNum,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,memberTagName)
	values
	(dutyID_,3,tmpMember_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 15 and parasID = 3));

    insert into netbar_duty_goods_catalog (goodsCatalog,dutyID,theNum,theSum,captureRatio,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,goodsCatalogName)
	select catalogID,dutyID_,sum(num),sum(consumeFee),format((sum(num)/onlineTimes_),2),1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_
    ,shiftName_
    ,(select catalogName from netbar_goods_catalog where gid = gid_ and catalogID = a.catalogID)
    from
	(select netbarGoodsID,(select catalogID from netbar_goods where netbarGoodsID = netbar_goods_order_detail.netbarGoodsID and gid = gid_) as catalogID,sum(theNum) as num ,sum((payFee-ifnull(couponDiscoutFee,0))*theNum) as consumeFee
	from netbar_goods_order_detail where goodsOrderID in (select  goodsOrderID from netbar_goods_order where gid = gid_ and state = 4 and actTime > beginDate_  and actTime <= endDate_) group by netbarGoodsID)
	as a group by catalogID;

	insert into netbar_duty_goods_type
	(goodsType,dutyID,theNum,theSum,captureRatio,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,goodsTypeName)
	values
	(1,dutyID_,goodsNum_,goodsConsume_,format((goodsNum_/onlineTimes_),2),1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 16 and parasID = 1));

	insert into netbar_duty_goods_type
	(goodsType,dutyID,theNum,theSum,captureRatio,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,goodsTypeName)
	values
	(2,dutyID_,watersNum_,waterConsume_,format((watersNum_/onlineTimes_),2),1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 16 and parasID = 2));

	insert into netbar_duty_funds (revenueType,dutyId,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,revenueTypeName) value (1,dutyID_,lastReserve_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netbar_paras where gid = gid_ and parasTypeID = 19 and parasID = 1));
	insert into netbar_duty_funds (revenueType,dutyId,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,revenueTypeName) value (2,dutyID_,memberCharge_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netbar_paras where gid = gid_ and parasTypeID = 19 and parasID = 2));
	insert into netbar_duty_funds (revenueType,dutyId,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,revenueTypeName) value (3,dutyID_,tmpFee_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netbar_paras where gid = gid_ and parasTypeID = 19 and parasID = 3));
	insert into netbar_duty_funds (revenueType,dutyId,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,revenueTypeName) value (4,dutyID_,tmpDeposit_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netbar_paras where gid = gid_ and parasTypeID = 19 and parasID = 4));
	insert into netbar_duty_funds (revenueType,dutyId,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,revenueTypeName) value (5,dutyID_,goodsFee_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netbar_paras where gid = gid_ and parasTypeID = 19 and parasID = 5));
	insert into netbar_duty_funds (revenueType,dutyId,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,revenueTypeName) value (6,dutyID_,waterBarFee_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netbar_paras where gid = gid_ and parasTypeID = 19 and parasID = 6));
	select ifnull(sum(onlineFee -couponDeduction),0) into onlineFee_ from netbar_online where state = 2 and offLineTime > beginDate_ and offLineTime <= endDate_;
	insert into netbar_duty_funds (revenueType,dutyId,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,revenueTypeName) value (7,dutyID_,onlineFee_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netbar_paras where gid = gid_ and parasTypeID = 19 and parasID = 7));
	insert into netbar_duty_funds_paytype (dutyID,payWay,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,payWayName) value (dutyID_,5,acount_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 2 and parasID = 5));
	insert into netbar_duty_funds_paytype (dutyID,payWay,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,payWayName) value (dutyID_,4,cashFee_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 2 and parasID = 4));
	insert into netbar_duty_funds_paytype (dutyID,payWay,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,payWayName) value (dutyID_,2,bankFee_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 2 and parasID = 2));
	insert into netbar_duty_funds_paytype (dutyID,payWay,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,payWayName) value (dutyID_,3,alipayFee_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 2 and parasID = 3));
	insert into netbar_duty_funds_paytype (dutyID,payWay,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,shiftName,payWayName) value (dutyID_,1,weixinFee_,1,gid_,shiftIDN_,dutyDate_,beginDate_,endDate_,shiftName_,(select theValue from netBar_paras where gid = gid_ and parasTypeID = 2 and parasID = 1));
 	insert into netbar_event(type,level2Type,gid,theValue,dataVersion,state,theOrder,eventDate,eventTime,completeTime)
 		  values(5,1,gid_,dutyID_,1,0,0,current_date(),current_time(),sysdate());
		  
	if result is null then
		set result=1;
		commit;
	else
		rollback;
	end if;
END ;;";
            return proc;
        }

        public static string procUpdateOnlineByCrossArea()
        {
            string proc = @"CREATE DEFINER=`admin`@`'%'` PROCEDURE `updateOnlineByCrossArea`(IN memberID_ bigint(20) unsigned,
    IN changePcTime_ bigint,
    IN onlineFee_ decimal(6,2),
    IN machineName_ VARCHAR(30),
    IN ruleID_ INT,
    IN areaID_ INT,
	IN tariffType_ TINYINT,
	IN idseq bigint unsigned,
    IN gid_ int,
    OUT result INT)
    SQL SECURITY INVOKER
    COMMENT '跨区域换机'
lab:BEGIN
    declare memberType_ tinyint;
	declare baseReserve_ decimal(8,2);
	declare awardReserve_ decimal(8,2);
	declare depositReserve_ decimal(8,2);
    declare machineID_ int;
	declare areaName_ varchar(30);
    declare billingID_ bigint;					
	declare onlineID_ bigint unsigned;
	declare payWay_ tinyint(1);
	declare startUser_ int;
    declare baseBalance_ decimal(8,2);
    declare awardBalance_ decimal(8,2);
	declare deposit_ decimal(8,2);	
    declare na_rows int;
    declare nm_rows int;
    declare nme_row int;
	declare no_rows int;
    declare currentCostTemp_ decimal(6,2);
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION set result=0;

	if tariffType_ != 1 then
		SET result = 14031;
		leave lab;
	end if;

	if changePcTime_ = 0 or changePcTime_ is null then
		SET result = 14032;
		leave lab;
	end if;
	
	if machineName_ = '' or machineName_ is null then
		SET result = 10622;
		leave lab;
	end if;

	select count(*) into nm_rows from netbar_machine where machineName = machineName_ and gid = gid_;
	if nm_rows > 1 then
		set result = 10621;
		leave lab;
	end if;
	
	select machineID into machineID_ from netbar_machine where machineName = machineName_ and gid = gid_;
	if machineID_ is null then
		set result = 10623;
		leave lab;
	end if;

	if areaID_ = 0 or areaID_ is null then
		set result = 10624;
		leave lab;
	end if;
	
	select count(*) into na_rows from netbar_area where areaID = areaID_ and gid = gid_ and state = 0;
	if na_rows = 0 then
		set result = 10625;
		leave lab;
	end if;
	
    select count(*) into nme_row from netbar_member where memberID = memberID_ and gid = gid_;
	if nme_row = 0 then
		set result = 1403;
		leave lab;
	end if;

	select baseBalance, awardBalance, deposit into baseReserve_, awardReserve_, depositReserve_ from netbar_member_account where memberID = memberID_ and gid = gid_;
	if baseReserve_ + awardReserve_ + depositReserve_ = 0 then
		set result = 14033;
		leave lab;
	end if;

    select count(*) into no_rows from netbar_online where memberID = memberID_ and offLineTime is null and gid = gid_;
    if no_rows > 1 then
		set result = 14034;
		leave lab;
    end if; 

	select memberType into memberType_ from netbar_member where memberID = memberID_ and gid = gid_;
	select areaName into areaName_ from netbar_area where areaID = areaID_ and gid = gid_ and state = 0;
	select onlineID, payWay, startUser into onlineID_, payWay_, startUser_ 
	from netbar_online where memberID = memberID_ and gid = gid_ and offLineTime is null; 
	select ifnull(sum(currentCostBase), 0), ifnull(sum(currentCostAward), 0), ifnull(sum(currentCostTemp), 0) into baseBalance_, awardBalance_, deposit_
	from netbar_billing where onlineID = onlineID_ and gid = gid_;
    select billingID into billingID_ 
	from netbar_billing where memberID = memberID_ and gid = gid_ order by billingID desc limit 1;

    start transaction;

	update netbar_online set
	onlineFee = onlineFee_, 
	baseReserve = baseReserve_, 
	awardReserve = awardReserve_, 
	baseBalance = baseBalance_, 
	awardBalance = awardBalance_, 
	deposit = deposit_, 
	dataVersion = dataVersion + 1,
	actTime = FROM_UNIXTIME(changePcTime_),	
	offLineTime = FROM_UNIXTIME(changePcTime_),
	state = 2
	where onlineID = onlineID_ and gid = gid_;

	update netbar_online set internetTime = TIMESTAMPDIFF(SECOND,onlineStartTime,offLineTime)
	where onlineID = onlineID_ and gid = gid_;         
	
	update netbar_billing set endTime = FROM_UNIXTIME(changePcTime_)
	where memberID = memberID_ and billingID = billingID_ and gid = gid_;
        
	if memberType_ = 1 then
		update netbar_online set depositState = 1, dataVersion = dataVersion + 1
		where onlineID = onlineID_ and gid = gid_;
	end if;

	insert into netbar_event(type,level2Type,gid,theValue,dataVersion,state,theOrder,eventDate,eventTime,completeTime) values (4,1,gid_,onlineID_,1,0,0,current_date(),current_time(),sysdate());

    insert into netbar_online
		(memberID,memberType,onlineRoomID,ifRoomOwner,state,gid,dataVersion,theDate,startUser,startCardTime,tariffConfigID,payWay,areaID,tariffType,authoriseUser)
	values
		(memberID_,memberType_,0,0,3,gid_,1,CURDATE(),startUser_,now(),0,payWay_,areaID_,tariffType_,0);
		
    select deposit into deposit_ from netbar_member_account where memberID = memberID_ and gid = gid_;
    if deposit_ > 0 then
		update netbar_online set depositState = 0, dataVersion = dataVersion + 1 
		where memberID = memberID_ and offLineTime is null and gid = gid_;
	end if;
	
	update netbar_online
	set state = 1, 
	onlineStartTime = FROM_UNIXTIME(changePcTime_),
	offLineTime = FROM_UNIXTIME(NULL),
	onlineFee = 0, 
	machineID = machineID_, 
	areaID = areaID_, 	
	areaName = areaName_,	
	tariffConfigID = 0, 
	ruleID = ruleID_,
	tariffDataVersion = 0, 
	machineName = machineName_, 
	tariffType = tariffType_, 
	onlineRoomID = idseq, 
	dataVersion = dataVersion + 1
	where memberID = memberID_ and gid = gid_ and offLineTime is null;    

	if result is null then
      set result = 1;
      commit;
	else
      rollback;
    end if;    
END ;;";
            return proc;
        }

        public static string procClearOnlineHistory()
        {
            string proc = @"CREATE DEFINER=`admin`@`'%'` PROCEDURE `clearOnlineHistory`(IN retentionDays_ INT,OUT result INT)
    SQL SECURITY INVOKER
    COMMENT '根据保留天数，清理netbar_online_history数据'
BEGIN
	DECLARE CONTINUE HANDLER FOR SQLEXCEPTION set result=0;
	start transaction;
	DELETE FROM netbar_online_history WHERE theDate <= DATE_SUB(CURRENT_DATE(),INTERVAL retentionDays_ DAY);
	DELETE FROM netbar_billing WHERE theDate <= DATE_SUB(CURRENT_DATE(),INTERVAL retentionDays_ DAY);
	
	if result is null then
		commit;
		set result = 1;
	else
		rollback;
	end if;
END ;;";
            return proc;
        }
    }
}
