#nullable enable
using GameCanvas;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class Game : GameBase
{   
    const int Mnum=2;
    int[] mos_x=new int[Mnum];
    int[] mos_y=new int[Mnum];
    int[] mos_speed=new int[Mnum];
    int[] mos_hp=new int[Mnum];
    bool[] boss=new bool[Mnum];
    bool[] alive=new bool[Mnum];
    int ball=1;
    float ball_x=340;
    float ball_y=700;
    float ball_speedy=7;
    float ball_speedx;
    bool power=false;
    bool Bspeed=false;
    int shoot=0;//1=シュート準備、2＝シュートしている間、0＝シュート終わった
    float fingx=0;
    float fingy=0;
    float tan=0;
    int hp=600;
    const int Cnum=3;
    int[] state = new int [Cnum];// state0==ENMPTY
    const int card_num=6;
    GcImage[] card_img=new GcImage[card_num];
    int waiting;
    int itemnum;
    int stage=1;


    public override void InitGame()
    {
        gc.SetResolution(720, 1280);
        for (int i =0 ; i < Mnum ; i ++){
            reset(i);
        }
        for (int i=0 ; i<Cnum;i++){
            state[i]=0;
        }
        set_cardimg();

    }

    public override void UpdateGame()
    {
        if (stage==1){

            for (int i =0 ; i < Mnum ; i ++){
                reset(i);
            }
            for (int i=0 ; i<Cnum;i++){
                state[i]=0;
            }
            set_cardimg();
            hp=600;

            if(gc.GetPointerX(0)>=140 && gc.GetPointerX(0)<=550 && gc.GetPointerY(0)>=700 && gc.GetPointerY(0)<=850){
                stage=2;
            }
            if(gc.GetPointerX(0)>=140 && gc.GetPointerX(0)<=550 && gc.GetPointerY(0)>=910 && gc.GetPointerY(0)<=1060){
                stage=3;
            }
        }

        if (stage==2){
            stage_f_up();
            if (hp<=0){
                stage=4;
            }
        }
        if(stage==3){
            if(gc.GetPointerX(0)>=70 && gc.GetPointerX(0)<=670 && gc.GetPointerY(0)>=1100 && gc.GetPointerY(0)<=1250){
                stage=2;
            }
        }
        if (stage==4){
            if(gc.GetPointerX(0)>=40 && gc.GetPointerX(0)<=440 && gc.GetPointerY(0)>=1100 && gc.GetPointerY(0)<=1250){
                stage=1;
            }
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();

        if (stage==1){
            gc.DrawImage(GcImage.Title,0,0);
            gc.DrawString("X: "+gc.GetPointerX(0),500,10);
            gc.DrawString("Y: "+gc.GetPointerY(0),500,30);
        }
        if (stage==2){
            stage_f_draw();
        }
        if(stage==3){
            gc.DrawImage(GcImage.Rule,0,0);
        }

        if (stage==4){
            gc.DrawImage(GcImage.End,0,0);
        }
    }

    void stage_f_up(){

        for(int i=0; i<Mnum;i++){
            mos_x[i]-=mos_speed[i];//moving 蚊 forward

            if (mos_x[i]<=-180){//蚊が画面越した処理
                if (boss[i]){
                    hp-=100;//my hp decrease
                }else{
                    hp-=50;//my hp decrease
                }
                reset(i);
            }

            if (mos_y[i]>=650){//死んだ蚊を消す
                reset(i);
            }
            
            if (mos_hp[i]<=0 && alive[i]==true){//蚊のhpが０以下なら死
                alive[i]=false;
                
                itemnum=gc.Random(0,9);
                if (itemnum==0){
                    waiting=1;
                }
                else if (itemnum==1){
                    waiting=2;
                }
                else if (itemnum==2){
                    waiting=3;
                }
                else if (itemnum==3){
                    waiting=4;
                }
                else if (itemnum==4){
                    waiting=5;
                }
            }

            if(!alive[i]){//死んだ蚊を落とす
                mos_y[i]+=7;
            }

            if (alive[i] && shoot==2){//当たった処理
                if (!boss[i]){
                    if(gc.CheckHitRect(ball_x,ball_y,75,75,mos_x[i],mos_y[i],164,75)){
                        HitPoint(i,ball);
                        if (ball==1){
                            reflect(i,164,75);
                        }
                    }
                }else{
                    if(gc.CheckHitRect(ball_x,ball_y,75,75,mos_x[i],mos_y[i],172,200)){
                        HitPoint(i,ball);   
                        if (ball==1){
                            reflect(i,164,75);
                        }
                    }
                }
            }
    
        } //mosquite の処理

        fingx=gc.GetPointerX(0);
        fingy=gc.GetPointerY(0);

        if (shoot==0){//最初

            get_item();

            ball_speed();

            if (700< gc.GetPointerY(0) && gc.GetPointerY(0) <800){
                shoot=1;
            }
        }

        float newx=0;
        float newy=0;

        if (shoot==1){//シュート準備
            newx=ball_x+37-fingx;//ボールからのx
            newy=fingy-ball_y+37;//ボールからのｙ
            if (gc.PointerEndCount==1){
                tan=newy/newx;
                shoot=2;
            }

        ball_speedx=ball_speedy/tan;
        }//shoot=1 シュートしている準備

        if (shoot==2){//シュートしている間
            ball_reflect();
            ball_x+=ball_speedx;
            ball_y-=ball_speedy;
            waitingbox();
            
        }

        if (shoot==3){//初期化
            ball=1;
            ball_y=700;
            power=false;
            Bspeed=false;
            /*
            if (used<3){
                state[used]=0;
            }
            */
            shoot=0;
        }

    }

    void stage_f_draw(){
        gc.DrawImage(GcImage.SkyOne, 0,0);
        for (int i=0; i<Mnum;i++){
            if (!boss[i]){
                if(alive[i]){
                    gc.DrawImage(GcImage.MosOne, mos_x[i], mos_y[i]);
                }else{
                    gc.DrawImage(GcImage.MosTwo, mos_x[i], mos_y[i]);
                }
            }else{
                if (alive[i]){
                    gc.DrawImage(GcImage.MosTri, mos_x[i], mos_y[i]);
                }else{
                    gc.DrawImage(GcImage.MosFour, mos_x[i], mos_y[i]);
                }
            } 
        }

        if (ball==1){
            gc.DrawImage(GcImage.BallOne, ball_x,ball_y);
        }
        else if (ball==2) {
            gc.DrawImage(GcImage.BallTwo, ball_x,ball_y);
        }
        else if (ball==3){
            gc.DrawImage(GcImage.BallTri, ball_x,ball_y);
        }
        if (shoot==1){
            gc.DrawLine(fingx,fingy,ball_x+37,ball_y+37);
        }

        gc.DrawImage(card_img[state[0]],50,900);
        gc.DrawImage(card_img[state[1]],270,900);
        gc.DrawImage(card_img[state[2]],500,900);

        gc.DrawString("sate[0]"+state[0],20,40);
        gc.DrawString("sate[1]"+state[1],20,60);
        gc.DrawString("sate[2]"+state[2],20,80);

        gc.DrawString("waiting"+waiting,20,100);

        gc.SetColor(255,0,0);
        gc.FillRect(20,20,hp,20);
        
    }

    void reset(int id){
        mos_y[id]=gc.Random(0,500);
        mos_x[id]=gc.Random(700,800);
        mos_speed[id]=gc.Random(1,3);
        if (gc.Random(0,10)==1){
            boss[id]=true;
        }
        else{
            boss[id]=false;
        }
        if (!boss[id]){
            mos_hp[id]=gc.Random(0,1000);
        }
        else if (boss[id]){
            mos_hp[id]=gc.Random(5000,10000);
        }
        alive[id]=true;

    }

    void HitPoint(int i,int ball){
        if (ball==1){
            if (!power){
                mos_hp[i]-=100;
            }else{
                mos_hp[i]-=200;
            }
        }
        else if (ball==2){
            if (!power){
                mos_hp[i]-=150;
            }else{
                mos_hp[i]-=300;
            }
        }
        else if (ball==3){
            if (!power){
                mos_hp[i]-=75;
            }else{
                mos_hp[i]-=150;
            }
            if(mos_speed[i]>0){
                mos_speed[i]-=1;
            }
            if (boss[i]){
                mos_hp[i]-=100;
            }
        }
    }

    void reflect(int id, int mos_w,int mos_h){//ball_speedyがpositive=上、
        if (ball_speedx>0 && ball_speedy>0 && (ball_y+75)<(mos_y[id]+ball_speedy)){
            ball_speedy*=-1;
        }//右下　箱の上　
        if (ball_speedx>0 && ball_speedy>0 && (ball_x+75)<(mos_x[id]+ball_speedx)){
            ball_speedx*=-1;
        }//右下　箱の左
        if (ball_speedx<0 && ball_speedy>0 && (ball_y+75)<(mos_y[id]+ball_speedy)){
            ball_speedy*=-1;
        }//左下　箱の上
        if (ball_speedx<0 && ball_speedy>0 && (ball_x)<(mos_x[id]+mos_w-ball_speedx)){
            ball_speedx*=-1;
        }//左下　箱の右
        if (ball_speedx>0 && ball_speedy<0 && (ball_y)>(mos_y[id]+mos_h-ball_speedy)){
            ball_speedy*=-1;
        }//右上　箱の下
        if (ball_speedx>0 && ball_speedy<0 && (ball_x+75)<(mos_x[id]+ball_speedx)){
            ball_speedx*=-1;
        }//右上　箱の左
        if (ball_speedx<0 && ball_speedy<0 && (ball_y)<(mos_y[id]+mos_h-ball_speedy)){
            ball_speedy*=-1;
        }//左上　箱の下
        if (ball_speedx<0 && ball_speedy<0 && (ball_x)<(mos_x[id]+mos_w-ball_speedx)){
            ball_speedx*=-1;
        }//左上　箱の右
    }

    void ball_reflect(){
        if (ball_x <= 0 && ball_speedx<0){
            ball_speedx*=-1;
        }

        else if(ball_x>=645 && ball_speedx>0){
            ball_speedx*=-1;
        }

        else if (ball_y<=0 && ball_speedy>0){
            ball_speedy*=-1;
        }

        else if (ball_y>=701){
            shoot=3;
        }
    }

    void ball_speed(){
        if (ball==1){
            if(!Bspeed){
                ball_speedy=7;
            }else{
                ball_speedy=10;
            }
        }
        if (ball==2){
            if(!Bspeed){
                ball_speedy=10;
            }else{
                ball_speedy=13;
            }
        }
        if (ball==3){
            if (!Bspeed){
                ball_speedy=6;
            }else{
                ball_speedy=9;
            }
        }
    }

    void get_item(){//state[i]==0=>Empty 1=>hp+=10 2=>poo 3=>speed 4=>shuri 5=>power
        if (fingy>=900 && fingy<=1100){

            if(250>=fingx && fingx >=50){
                what_item(state[0]);
                state[0]=0;
            }
            else if(470>=fingx && fingx >=270){
                what_item(state[1]);
                state[1]=0;
            }
            else if(700>=fingx && fingx >=500){
                what_item(state[2]);
                //used=2;
                state[2]=0;
            }
        }
    }
    void what_item(int item){
        if(item==1){
            hp+=100;
        }
        else if(item==2){
            ball=3;
        }
        else if(item==3){
            Bspeed=true;
        }
        else if(item==4){
            ball=2;
        }
        else if(item==5){
            power=true;
        }
    }

    void set_cardimg(){
        for (int i=0; i<card_num; i++){
            if (i==0){
                card_img[i]=GcImage.CardZero;
            }
            else if(i==1){
                card_img[i]=GcImage.CardTwo;
            }
            else if(i==2){
                card_img[i]=GcImage.CardTri;
            }
            else if(i==3){
                card_img[i]=GcImage.CardOne;
            }
            else if(i==4){
                card_img[i]=GcImage.CardFour;
            }
            else if(i==5){
                card_img[i]=GcImage.CardFive;
            }
        }
    }

    void waitingbox(){
        if(state[0]==0){
            state[0]=waiting;
            waiting=0;
        }else if(state[1]==0){
            state[1]=waiting;
            waiting=0;
        }else if(state[2]==0){
                state[2]=waiting;
                waiting=0;
        }
    } 
    
}