select t.Tier, t.Player, s.Streak, s.Winrate, s.LastSeen
from Tiers t
left join streaks s on s.player = t.Player and s.tier = t.Tier
where
	Streak is null or
	Streak = (select max(Streak) from Streaks z where z.Player = s.Player and z.Tier = s.Tier)
order by
	case when t.Tier = 'X' then 1 else 0 end desc,
	case when t.Tier = 'S' then 1 else 0 end desc,
	case when t.Tier = 'A' then 1 else 0 end desc,
	case when t.Tier = 'B' then 1 else 0 end desc,
	case when t.Tier = 'P' then 1 else 0 end desc,
	cast(isnull((select max(Streak) from Streaks z where z.Player = s.Player and z.Tier = s.Tier),0) as int) desc,
winrate desc,
lastseen desc