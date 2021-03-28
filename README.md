# StockPressure

The purpose of this application is to find some metric for measuring the ***potential*** pressure that a stock might be under from the effect of entities extensively short-selling it.

Normally, a commodity is sold and money is exchanged. The commodity is transferred in one transaction while the money is exchanged in a second one. A short sell is when the commodity cannot be exchanged at the specific moment -- either because they don't actually have it yet, or there is a technical difficulty for delivering it -- but the money is exchanged anyway. In these cases, an IOU is written that behaves JUST like the original commodity, except that the IOU must be delivered in a specific window of time.

For one or two shares, this is not a big deal. The market will course-correct. However, if someone games the system and sells more than a specific amount of IOUs, then when it comes time for the IOU to come due, there may not be enough shares on the market for the borrower to cover. At that point, they are under pressure to ***find*** one and pay for it at any cost.

Generally this is done by calculating the Short Percent -- the percent of items shorted in this way compared to the available shares on the market, called the Float. The big problem is that there are multiple ways to trade, and to borrow ***other people's borrowed shares***, which dilutes and skews any calculation I may make.

Either way, it is this pressure is what I hope to use FINRA data to derive, and I think I have found a way to do so.

Let's assume that ALL shares that the FINRA knows about (it doesn't track them all, I know... but bear with me) go to cover these IOUs, called short positions. I know this isn't true, but let's assume that to give them the best position. I only have two stats to look at from the FINRA -- Total Volume and Short Volume. Using those, I can then calculate a ***guess*** at the short percent as follows:

If a short sell happens, it should trigger an increase in Total Volume.

Then, when the sale to COVER that short sell happens, it should trigger ANOTHER increase in Total Volume.

So, let's look at the Total Volume, and Short Volume values when only three things happen on a stock in one day: One normal sale, and one **covered** short sell and one **uncovered** short sell. I don't care about additional normal sales, because we're going to assume that THEY bought them and will apply it to their short position at some point in the future.

When the normal sale goes through, Total Volume is increased by one. But since it was a normal sale, Short Volume is not.

`TV = 1, SV = 0`

Then, someone BORROWS a share and short-sells it. This counts as both a SHORT and a SELL, thus both numbers should increase.

`TV = 2, SV = 1`

Then, someone SELLS that share and to cover their position. Again, this is just a normal sell, so it should only increase the total volume by 1.

`TV = 3, SV = 1`

And finally, someone short-sells another one, increasing both the TOTAL and SHORT counters, but no follow-up "sell" is posted.

`TV = 4, SV = 2`

At the end of this very simplistic day, that is our total. Now, I can ASSUME that all non-shorted sells went to cover shorted positions. This would give the short-ers the strongest-possible position, even if that wasn't what actually happened. In this case, any other combination of numbers and actualities means that they are WEAKER than the possible value that I calculate using this prediction.

So, let's look at Possible Short Position based on these assumptions:

`PSP = (2 * SV)`- TV

In our one-day case above:

`PSP = (2 * SV) - TV = (2 * 2) - 4 =  0`

In this case, the number is zero, meaning they mathematically COULD have covered all their bases. Whether or not they did is irrelevant. They COULD have, and there is no Squeeze. SOMEONE out in the universe has enough shares to bust our hand and sell if they haven't already. I mean, **I** didn't sell any... right? That volume had to come from SOMEWHERE.

If the number is negative, that means simply that there were MORE than enough shares sold to cover.

Now, let's say I see something like `TV = 8, SV = 5`. What would that tell me based on the equation I have above?

`PSP = (2 * SV) - TV = (2 * 5) - 8 =  +2`

This equation is the short-ers STRONGEST possible position that every single non-short sale went to cover their shorts. But now the result is POSITIVE. That means there is no mathematical way (with the numbers given) they could have covered their short positions. Anything left over can actually be carried forward into the next day.... where I do this all over again, adding in the PREVIOUS day's carry-over.

`Total PSP = (TV - (2 * SV)) + CarryOver`

Now that I have the Total Possible Short Position, what percentage is that of the Float? So, if there were only 5 total available shares in the wild, the Total PSP of 2 is simply 2 / 5, or 0.4, or 40%.

This is not to say that the Short Percent is REALLY 40%. What it is saying, is that 40% short is the short-er's BEST POSSIBLE scenario. Any other version of reailty only makes their position weaker. They would likely be feeling a little pressure at that point. There are still enough shares in the market aside from my 1 share to cover, but that's going to be a little harder.

The reality is that at 40%, they could probably wiggle out of it without buying MY shares, but the price of the stock will increase in the process, because they will need to buy a significant portion of the available market JUST to cover their position.

But what if I end up with values that show the Share Percent could ***possibly*** be insane values like 150% or 160%? Again, this is the short-er's BEST position, meaning that in reality it is ***MUCH*** worse. While I don't really know *how* much worse, I do know that this isn't good for them, but great for me. They MUST buy all of the stocks they can, wait for someone to sell... then do it at ***least*** half-again to cover their position. They are ***very*** likely to have to buy ***my*** shares at whatever price I set.

# My App

This little app is designed to display all of this by taking in a slice of these two numbers that the FINRA has on file for each day. If there is any carry over, it will do so, increasing the percentage as-needed. If -- after accumulating the day's values -- they ever go negative, it resets the pressure value to 0 because there **is no** pressure. 

Again, I'm not measuring Short Percent, I'm trying to measure their BEST possible position. That's what will tell me if the squeeze is still on. If their BEST position is still under a tremendous amount of pressure, then I know that things are still good for a squeeze to continue into the next day.

Thus, I'm not calling this "short"-anything. I'm not measuring short-ness. I'm measuring pressure they are under to COVER their shorts. That's why I'm calling this value simply "Pressure".

Now, what pressure do you believe will be enough to put a significant squeeze on the stock? 70%? Okay, then when they are at 60% pressure, then the following formula applies:

`60% (current) / 70% (max to continue the squeeze) = Pressure Ratio = 85%`

That is to say, they are at 85% of the possible pressure needed to force the squeeze at 70%. What happens when GME reports at 150% sold? I know that their position -- in actuality -- is worse than 150%, but I can be confident that they are at least :

`150% / 70% = Pressure Ratio = 214%`

They are at 214% of what it would take to drive the price up once the engines ignite. After the engine ignites, they will relieve some of their pressure by buying up stock (or the DTCC will do that for them) to cover their shorts. At the end of each day, I can check this supposed pressure to at least see if I can look forward to more pressure tomorrow, or if the squeeze has ***possibly*** squoze.