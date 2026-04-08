
create database ShinewaySCADA
use ShinewaySCADA


create table t_Machine(
	MachineID int not null  primary key  identity,
	FullName varchar(255)   not null,
	ShortName varchar(255)  not null,
	IpAddr varchar(255)  not null,
	PortNum int not null,
	Status int not null  default(1), -- -1=删除，1=正常
	OrderBy  int
	--不设置unique 灵活操作Status
)
insert into t_Machine(FullName,ShortName,IpAddr,PortNum,OrderBy) values 
select MachineID as '序号',FullName as '全称',ShortName as '简称',IpAddr as 'IP地址',PortNum as '端口号',Status as '状态',OrderBy as '排序' from t_Machine

drop table t_WkcntrMinute

--每分钟的记录
create table t_WkcntrMinute(
	ID bigint not null  primary key  identity,
	MachineID  int not null,
	WkcntrNO  varchar(255) not null,		--//20231101 0820(12)
	Wkcntr  float not null,					--//当前计数 (-1 负数时异常)
	LedStatus  int not null  default(-1),	--//-1异常 1绿 2黄 3红 4其他
	ItemName  varchar(255),
	RecordingTime datetime  not null default(GETDATE()),
	unique(MachineID,WkcntrNO),
	foreign key (MachineID) references  t_Machine(MachineID)
)


delete from t_WkcntrMinute



select count(*) from t_WkcntrMinute where MachineID=1
select count(*) from t_WkcntrMinute where MachineID=30
select count(*) from t_WkcntrMinute where MachineID=50
select count(*) from t_WkcntrMinute where MachineID=80
select count(*) from t_WkcntrMinute where MachineID=103

insert into t_WkcntrMinute(MachineID,MachineCount,MachineSum,MachineLed,MachineItem,DayStr,DayHourStr,DayHourMinuteStr,DayHourMinuteSecondStr,DayHourMinuteSecond10Str)(100,'-1','0','-1','','20231114','2023111412','202311141250','20231114125050','2023111412505'),('101','-1','0','-1','','20231114','2023111412','202311141250','20231114125050','2023111412505'),('102','-1','0','-1','','20231114','2023111412','202311141250','20231114125050','2023111412505'),('103','-1','0','-1','','20231114','2023111412','202311141250','20231114125050','2023111412505')

select * from t_WkcntrMinute where WkcntrNO='2023111413582' order by RecordingTime desc


delete from t_WkcntrMinute

select WkcntrNO , count(WkcntrNO)  from t_WkcntrMinute group by WkcntrNO order by WkcntrNO


select * from t_WkcntrMinute where WkcntrNO ='202311141459' order by MachineID








