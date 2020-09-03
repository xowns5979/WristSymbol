## 1. Print AC and RT for log data and write it into .csv file.
library(dplyr)

names = c("p1","p2","p3","p4","p5","p6","p7","p8","p9","p10","p11","p12")
referenceframe = c("handNorth","watchNorth")
#group = c("digit", "alphabet")
#strategy = c("baseline","hetero")
#armposture = c("armFront","armBody")
armposture = c("armFront","armBody","armDown")
mode = c("training","main")

base_df = data.frame()
for (i in 1:12){
  for(j in 1:2){
    for(k in 1:3){
      #for(p in 1:2){
        for(q in 2:2){
          file_name = paste("data/",names[i] ,"_",referenceframe[j],"_",armposture[k],"_", mode[q], ".csv",sep="")
          #file_name = paste("data/",names[i] ,"_",group[j],"_",strategy[k],"_",armposture[p],"_", mode[q], ".csv",sep="")
          file_data = read.csv(file_name, header=T, stringsAsFactors = F)
          base_df = rbind(base_df,file_data)
        }
      #}
    }
  }
}
base_df$id = factor(base_df$id, levels=names)
base_df$rt = base_df$enterstamp - base_df$playendstamp

base_df

#result = group_by(base_df, id, group, strategy, armpose, mode) %>%
result = group_by(base_df, id, orientation, armpose) %>%
    summarise(
    count = n(),
    correct = mean(correct)*100,
    reactTime = mean(rt)
  )
print(result,n=100)
write.csv(result, "result.csv")

## 2. Print AC of each 3 and 4 and 5 Vibration letter
library(dplyr)

names = c("p1","p2","p3","p4","p5","p6","p7","p8","p9","p10","p12")
group = c("digit", "alphabet")
strategy = c("baseline","hetero")
armposture = c("armFront","armBody")
mode = c("training","main")

base_df = data.frame()
for (i in 1:11){
  for(j in 2:2){
    for(k in 1:2){
      for(p in 1:2){
        for(q in 2:2){
          file_name = paste("data/",names[i] ,"_",group[j],"_",strategy[k],"_",armposture[p],"_", mode[q], ".csv",sep="")
          file_data = read.csv(file_name, header=T, stringsAsFactors = F)
          base_df = rbind(base_df,file_data)
        }
      }
    }
  }
}
base_df$id = factor(base_df$id, levels=names)
base_df$rt = base_df$enterstamp - base_df$playendstamp


# alphabet group
base_df_3vib = base_df[which(base_df$realPattern == "a" | base_df$realPattern == "c" | base_df$realPattern == "f" |
                               base_df$realPattern == "j" | base_df$realPattern == "l" | base_df$realPattern == "r" |
                               base_df$realPattern == "t" | base_df$realPattern == "v"),]
base_df_4vib = base_df[which(base_df$realPattern == "b" | base_df$realPattern == "d" | base_df$realPattern == "e" |
                               base_df$realPattern == "h" | base_df$realPattern == "n" | base_df$realPattern == "p" |
                               base_df$realPattern == "s" | base_df$realPattern == "u" | base_df$realPattern == "x" |
                               base_df$realPattern == "y" | base_df$realPattern == "z"),]
base_df_5vib = base_df[which(base_df$realPattern == "g" | base_df$realPattern == "k" | base_df$realPattern == "m" |
                               base_df$realPattern == "o" | base_df$realPattern == "w"),]

result_3vib = group_by(base_df_3vib, group, strategy, armpose, mode) %>%
  summarise(
    count = n(),
    correct = mean(correct)*100,
    reactTime = mean(rt)
  )
print(result_3vib,n=100)

result_4vib = group_by(base_df_4vib, group, strategy, armpose, mode) %>%
  summarise(
    count = n(),
    correct = mean(correct)*100,
    reactTime = mean(rt)
  )
print(result_4vib,n=100)

result_5vib = group_by(base_df_5vib, group, strategy, armpose, mode) %>%
  summarise(
    count = n(),
    correct = mean(correct)*100,
    reactTime = mean(rt)
  )
print(result_5vib,n=100)
