SELECT TOP 1000 [Player]
      ,[Wager]
      ,[Date]
      ,[Balance]
      ,Result
  FROM [sentro].[dbo].[Bets-temp] where result > -100001
  
select 1 WagerLevel, sum(case when result > 0 then 1 else 0 end) BetsWon, count(*) TotalBets, SUM(Result) Profit from [bets-temp] where wager = 1			and  result > -100001
select 10000 WagerLevel, sum(case when result > 0 then 1 else 0 end) BetsWon, count(*) TotalBets, SUM(Result) Profit from [bets-temp] where wager = 10000		and  result > -100001
select 30000 WagerLevel, sum(case when result > 0 then 1 else 0 end) BetsWon, count(*) TotalBets, SUM(Result) Profit from [bets-temp] where wager = 30000		and  result > -100001
select 50000 WagerLevel, sum(case when result > 0 then 1 else 0 end) BetsWon, count(*) TotalBets, SUM(Result) Profit from [bets-temp] where wager = 50000		and  result > -100001
select 100000 WagerLevel, sum(case when result > 0 then 1 else 0 end) BetsWon, count(*) TotalBets, SUM(Result) Profit from [bets-temp] where wager = 100000		and  result > -100001


select Wager, sum(case when Profit > 0 then 1 else 0 end) BetsWon, count(*) TotalBets,
cast((cast(sum(case when Profit > 0 then 1 else 0 end) as decimal(10,0)) / count(*)) * 100 as decimal(5,2)),
SUM(Profit) Profit, MAX(Profit) BestResult, MIN(Profit) WorstResult, AVG(Profit) AverageResult
from Bets
where Profit > -100001 and Profit < 1000000 and (wager = 1 or wager >= 10000)
group by Wager

select Wager, sum(case when result > 0 then 1 else 0 end) BetsWon, count(*) TotalBets, SUM(Result) Profit from [bets-temp]
where result > -100001 and (wager = 1 or wager >= 10000) and Date > '2016-04-13 09:00' and Date < '2016-04-13 13:25'
group by Wager

select Wager, sum(case when result > 0 then 1 else 0 end) BetsWon, count(*) TotalBets, SUM(Result) Profit from [bets-temp]
where result > -100001 and (wager = 1 or wager >= 10000) and Date > '2016-04-13 13:25'
group by Wager

select * from Bets where Profit = 1222460