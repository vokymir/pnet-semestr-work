# pnet-semestr-work

Bird-watching application for enthusiast written in C#.

## Idea

The app would have list of all Users and Birds.

Each User can create multiple Watchers. That's for having children e.g.
Each User can create multiple Events.

Each Event stores its participants (Watchers) and vice versa.
Also, each Watcher and Event store its Users, making
m:n relation between each and every User-Watcher-Event -> a triangle.

Each Watcher can see a Bird and therefore Record it.
Each Watcher have list of Records.
Each Record is just a Bird reference and meta-data (e.g. DateTime).

Each Event asks Watchers, if they saw birds fitting criteria in the timespan.
Criteria such as: not repeating species in event per user, only seen in the night,...

For this, EventHandlerers should be implemented and something like INotifyPropertyChanged or so.
