# Redis Schema

- `allpowers`: Set of all power IDs
- `allteams`: Set of all team IDs
- `allheroes`: Set of all hero IDs
- `power:[powerId]`: Hash for a single power
	- `name`: power's name
- `team:[teamId]`: Hash for a single team
	- `team`: team's name
- `teammembers:[teamId]`: Set of hero IDs that are members of a single team
- `hero:[heroId]`: Hash for a single hero
	- `name`: hero's name
	- `location`: hero's location
- `heropowers:[heroId]`: Set of power IDs posessed by a single hero