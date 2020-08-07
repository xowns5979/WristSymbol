library(dplyr)


# Names
names = c("p1","p2","p3","p4","p5","p6","p7","p8","p9","p10","p11","p12")
referenceFrame = c("handNorth","watchNorth")
pose = c("armFront","armBody","armDown")
mode = c("training","main")
# 1. 1 Letter Accuracy [%]  

base_df = data.frame()
for(p in 2:2){
  for(k in 1:3){
    for(j in 1:2){
      for (i in 1:12){
        file_name = paste("Exp13_data/",names[i] ,"_", referenceFrame[j],"_",pose[k],"_",mode[p], ".csv",sep="")
        file_data = read.csv(file_name, header=T, stringsAsFactors = F)
        base_df = rbind(base_df,file_data)
      }  
    }  
  }  
}
base_df
base_df$id = factor(base_df$id, levels=names)

base_df$rt = base_df$enterstamp - base_df$playendstamp

base_df_handNorth = base_df[base_df$orientation=="handNorth",]
base_df_watchNorth = base_df[base_df$orientation=="watchNorth",]


result = group_by(base_df, orientation, realPattern) %>%
  summarise(
    count = n(),
    correct = mean(correct)*100,
    rt = mean(rt)
    #diff_1 = sum(diffCount_1),
    #diff_2 = sum(diffCount_2),
    #diff_3 = sum(diffCount_3)
    #rt = mean(reaction_time)
    #sd = sd(correct, na.rm = TRUE)*100
  )
print(result,n=100)
write.csv(result, "result.csv")
