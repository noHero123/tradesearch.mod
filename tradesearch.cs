using System;

using ScrollsModLoader.Interfaces;
using UnityEngine;
using Mono.Cecil;
//using Mono.Cecil;
//using ScrollsModLoader.Interfaces;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;
using JsonFx.Json;
using System.Text.RegularExpressions;



namespace tradesearch.mod
{



    /*struct playerinfo 
    {
        public string ID;
        public string name;
    }*/

    public class tradesearch : BaseMod
	{

        private GUISkin chatSkin;

        private TradeSystem ts = null; // battlemode used for sending the chat
        private Lobby lby = null;
        private InvocationInfo lbyinfo = null;
        private List<Card> p1cards = new List<Card>();//pointer to Lobby.p1cards
        private List<Card> p2cards = new List<Card>();
        private List<Card> orgicardsPlayer1 = new List<Card>();//copy of orginal cards
        private List<Card> orgicardsPlayer2 = new List<Card>();
        private List<Card> p1moddedlist = new List<Card>(); // filtered cards 
        private List<Card> p2moddedlist = new List<Card>();
        private List<Card> p1xtraaddedcards = new List<Card>();//cards that are not in filter, but in offer window
        private List<Card> p2xtraaddedcards = new List<Card>();
        private Boolean searchedself = false;
        private Boolean searchedoppo = false;
        private Boolean trading = false;
        string orginalp1name = "";
        string orginalp2name = "";
        int orginalint = 0;
        int anzupdates = 0;
        TradeInfo player1=new TradeInfo();
        TradeInfo player2 = new TradeInfo();
        private string traderoomname = "";
        private string selfsearchstring = "";
        private string opposearchstring = "";
        private float menueheight = 50;
        private ScrollsFrame outerFrame1;
        private ScrollsFrame outerFrame2;
        private Rect p1rectsearchmenu;
        private Rect p2rectsearchmenu;
        private Rect p1searchrect;
        private Rect p2searchrect;

        private Rect p1growthrect;
        private Rect p1orderrect;
        private Rect p1energyrect;
        private Rect p1decayrect;
        private Rect p1commonrect;
        private Rect p1uncommonrect;
        private Rect p1rarerect;
        private Rect p1mt3rect;
        private Rect p1clearrect;

        private bool p1growthbool=true;
        private bool p1orderbool = true;
        private bool p1energybool = true;
        private bool p1decaybool = true;
        private bool p1commonbool = true;
        private bool p1uncommonbool = true;
        private bool p1rarebool = true;
        private bool p1mt3bool = false;


        private Rect p2growthrect;
        private Rect p2orderrect;
        private Rect p2energyrect;
        private Rect p2decayrect;
        private Rect p2commonrect;
        private Rect p2uncommonrect;
        private Rect p2rarerect;
        private Rect p2mt3rect;
        private Rect p2clearrect;

        private bool p2growthbool = true;
        private bool p2orderbool = true;
        private bool p2energybool = true;
        private bool p2decaybool = true;
        private bool p2commonbool = true;
        private bool p2uncommonbool = true;
        private bool p2rarebool = true;
        private bool p2mt3bool = false;
        Texture2D growthres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_growth");
        Texture2D energyres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_energy");
        Texture2D orderres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_order");
        Texture2D decayres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_decay");
        

         private GUISkin chatButtonSkin;
        private GUIStyle chatLogStyle;

		//initialize everything here, Game is loaded at this point4
        public tradesearch()
		{

            
    
		}

        


		public static string GetName ()
		{
            return "tradesearch";
		}

		public static int GetVersion ()
		{
			return 1;
		}

        private void addofferedcardsoppo()
        { // adds offered cards to pxmoddedlists,if they are filtered not in it (otherwise they are not shown in offered window )

            this.p2xtraaddedcards.Clear();
            //same thing for player2 stuff
            long[] cardIds = player2.cardIds;
            for (int i = 0; i < cardIds.Length; i++)
            {
                bool found = false;
                foreach (Card c in this.p2moddedlist) //search Id in p1moddedlist
                {
                    if (c.getId() == cardIds[i])
                    {
                        found = true;
                        break;
                    };
                }

                if (found == false) //if Id is not in p1moddedlist, add it from orgicardsplayer1
                {
                    foreach (Card c in this.orgicardsPlayer2)
                    {
                        if (c.getId() == cardIds[i])
                        {
                            this.p2xtraaddedcards.Add(c);
                            break;
                        };

                    }

                }

            }

        }

        private void addofferedcardsself()
        { // adds offered cards to pxmoddedlists,if they are filtered not in it (otherwise they are not shown in offered window )
            this.p1xtraaddedcards.Clear();
            long[] cardIds = player1.cardIds;
            for (int i = 0; i < cardIds.Length; i++)
            {
                bool found = false;
                foreach (Card c in this.p1moddedlist) //search Id in p1moddedlist
                {
                    if (c.getId() == cardIds[i])
                    {
                        found = true;
                        break;
                    };
                }

                if (found == false) //if Id is not in p1moddedlist, add it from orgicardsplayer1
                {
                    foreach (Card c in this.orgicardsPlayer1)
                    {
                        if (c.getId() == cardIds[i])
                        {
                            this.p1xtraaddedcards.Add(c);
                            break;
                        };

                    }

                }

            }

            

        }

        private void addofferedcards()
        { // adds offered cards to pxmoddedlists,if they are filtered not in it (otherwise they are not shown in offered window )
            this.p1xtraaddedcards.Clear();
            this.p2xtraaddedcards.Clear();
            long[] cardIds = player1.cardIds;
            for (int i = 0; i < cardIds.Length; i++)
            {
                bool found = false;
                foreach (Card c in this.p1moddedlist) //search Id in p1moddedlist
                {
                    if (c.getId() == cardIds[i]) 
                    {
                        found = true;
                        break;
                    };
                }

                if (found == false) //if Id is not in p1moddedlist, add it from orgicardsplayer1
                {
                    foreach (Card c in this.orgicardsPlayer1) 
                    {
                        if (c.getId() == cardIds[i])
                        {
                            this.p1xtraaddedcards.Add(c);
                            break;
                        };
                    
                    }
                
                }
            
            }

            //same thing for player2 stuff
            cardIds = player2.cardIds;
            for (int i = 0; i < cardIds.Length; i++)
            {
                bool found = false;
                foreach (Card c in this.p2moddedlist) //search Id in p1moddedlist
                {
                    if (c.getId() == cardIds[i])
                    {
                        found = true;
                        break;
                    };
                }

                if (found == false) //if Id is not in p1moddedlist, add it from orgicardsplayer1
                {
                    foreach (Card c in this.orgicardsPlayer2)
                    {
                        if (c.getId() == cardIds[i])
                        {
                            this.p2xtraaddedcards.Add(c);
                            break;
                        };

                    }

                }

            }
        
        }

        private void updatetradeoppo()
        {
            p2cards.Clear();
            this.addofferedcardsoppo();
            p2cards.AddRange(this.p2moddedlist);
            p2cards.AddRange(this.p2xtraaddedcards);
            this.ts.UpdateView(false, this.player1, this.player2);


        }

        private void updatetradeself()
        {
           
            p1cards.Clear();
            this.addofferedcardsself();
            p1cards.AddRange(this.p1moddedlist);
            p1cards.AddRange(this.p1xtraaddedcards);
            this.ts.UpdateView(false, this.player1, this.player2);
            

        }

        private void updatetrade() {
            //Console.WriteLine(this.orgicardsPlayer1.Count+" updatetrade");
            //string trdroom = this.ts.GetTradeRoomName();
            //this.ts.StartTrade(this.p1moddedlist, this.p2moddedlist, this.orginalp1name, this.orginalp2name, this.orginalint);
            //this.ts.SetTradeRoomName(trdroom);
            //List<Card> p1cards = (List<Card>)typeof(Lobby).GetField("cardsPlayer1", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(lbyinfo.target);
            //List<Card> p2cards = (List<Card>)typeof(Lobby).GetField("cardsPlayer2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(lbyinfo.target);
            p1cards.Clear();
            p2cards.Clear();
            this.addofferedcards();
            p1cards.AddRange(this.p1moddedlist);
            p2cards.AddRange(this.p2moddedlist);
            p1cards.AddRange(this.p1xtraaddedcards);
            p2cards.AddRange(this.p2xtraaddedcards);
            this.ts.UpdateView(false, this.player1, this.player2);
            //this.ts.AddP1CardsToInventory();
            //this.ts.AddP2CardsToInventory();
        
        }

        private void searchmorethan3()
        {
            Dictionary<string, int> available= new Dictionary<string,int>();
            foreach (Card card in this.orgicardsPlayer1)
            {
                if (!available.ContainsKey(card.getName())) 
                {
                    available.Add(card.getName(), 0);
                }
                available[card.getName()] = available[card.getName()]+1;
            }


            List<Card> temp = new List<Card>(p1moddedlist);
            this.p1moddedlist.Clear();
            foreach (Card card in temp)
            {
                if (card.tradable == true && available[card.getName()] > 3)
                {
                    available[card.getName()] = available[card.getName()] - 1;
                    this.p1moddedlist.Add(card); 
                };

            }

        }

        private void searchmorethan3oppo()
        {
            Dictionary<string, int> available = new Dictionary<string, int>();
            foreach (Card card in this.orgicardsPlayer2)
            {
                if (!available.ContainsKey(card.getName()))
                {
                    available.Add(card.getName(), 0);
                }
                available[card.getName()] = available[card.getName()] + 1;
            }


            List<Card> temp = new List<Card>(p2moddedlist);
            this.p2moddedlist.Clear();
            foreach (Card card in temp)
            {
                if (card.tradable == true && available[card.getName()] > 3)
                {
                    available[card.getName()] = available[card.getName()] - 1;
                    this.p2moddedlist.Add(card);
                };

            }

        }

        private void searchforownenergy(string[] rare)
        {
            List<Card> temp = new List<Card>(p1moddedlist);
            this.p1moddedlist.Clear();
            foreach (Card card in temp)
            {
                if (rare.Contains(card.getResourceString().ToLower())) { this.p1moddedlist.Add(card); };

            }

        }

        private void searchforoppoenergy(string[] rare)
        {
            List<Card> temp = new List<Card>(p2moddedlist);
            this.p2moddedlist.Clear();
            foreach (Card card in temp)
            {
                if (rare.Contains(card.getResourceString().ToLower())) { this.p2moddedlist.Add(card); };

            }

        }

        private void searchforownrarity(int[] rare)
        {
            List<Card> temp = new List<Card>(p1moddedlist);
            this.p1moddedlist.Clear();
            foreach (Card card in temp)
            {
                if (rare.Contains(card.getRarity())) { this.p1moddedlist.Add(card); };

            }

        }

        private void searchforopporarity(int[] rare)
        {
            List<Card> temp = new List<Card>(p2moddedlist);
            this.p2moddedlist.Clear();
            foreach (Card card in temp)
            {
                if (rare.Contains(card.getRarity())) { this.p2moddedlist.Add(card); };

            }

        }

        private void searchforname(string name) 
        {
            this.p1moddedlist.Clear();
            foreach(Card card in this.orgicardsPlayer1)
            {
                if (card.getName() == name) { this.p1moddedlist.Add(card); };
            
            }
        
        }

        private void searchopponentforname(string name)
        {
            this.p2moddedlist.Clear();
            foreach (Card card in this.orgicardsPlayer2)
            {
                if (card.getName() == name) { this.p2moddedlist.Add(card); };

            }

        }

        private void containsname(string name)
        {
            List<Card> temp = new List<Card>(p1moddedlist);
            this.p1moddedlist.Clear();
            foreach (Card card in temp)//this.orgicardsPlayer1)
            {
                if (card.getName().ToLower().Contains(name.ToLower())) { this.p1moddedlist.Add(card); };

            }

        }

        private void containsopponentname(string name)
        {
            List<Card> temp = new List<Card>(p2moddedlist);
            this.p2moddedlist.Clear();
            foreach (Card card in temp)//this.orgicardsPlayer2)
            {
                if (card.getName().ToLower().Contains(name.ToLower())) { this.p2moddedlist.Add(card); };

            }

        }  
      
        private void onlytradeable() // select only tradeable cards (self and opponent)
        {
            List<Card> temp = new List<Card>(p1moddedlist);
            this.p1moddedlist.Clear();
            foreach (Card card in temp)//this.orgicardsPlayer1)
            {
                if (card.tradable) { 
                    this.p1moddedlist.Add(card);
                    //Console.WriteLine(card.getName());
                };

            }
            temp = new List<Card>(p2moddedlist);
            this.p2moddedlist.Clear();
            foreach (Card card in temp)//this.orgicardsPlayer2)
            {
                if (card.tradable) { this.p2moddedlist.Add(card); };

            }

        }

        private void onlytradeableself()
        {
            List<Card> temp = new List<Card>(p1moddedlist);
            this.p1moddedlist.Clear();
            foreach (Card card in temp)//this.orgicardsPlayer1)
            {
                if (card.tradable)
                {
                    this.p1moddedlist.Add(card);
                    //Console.WriteLine(card.getName());
                };

            }
            
        }

        private void onlytradeableoppo()
        {
            List<Card> temp = new List<Card>(p1moddedlist);
            temp = new List<Card>(p2moddedlist);
            this.p2moddedlist.Clear();
            foreach (Card card in temp)//this.orgicardsPlayer2)
            {
                if (card.tradable) { this.p2moddedlist.Add(card); };

            }

        }



		//only return MethodDefinitions you obtained through the scrollsTypes object
		//safety first! surround with try/catch and return an empty array in case it fails
		public static MethodDefinition[] GetHooks (TypeDefinitionCollection scrollsTypes, int version)
		{
            try
            {
                return new MethodDefinition[] {
                  scrollsTypes["Communicator"].Methods.GetMethod("sendRequest", new Type[]{typeof(Message)}),                  
                  scrollsTypes["Lobby"].Methods.GetMethod("Start")[0],
                  scrollsTypes["TradeSystem"].Methods.GetMethod("StartTrade", new Type[]{typeof(List<Card>) , typeof(List<Card>), typeof(string), typeof(string), typeof(int)}),
                  scrollsTypes["TradeSystem"].Methods.GetMethod("UpdateView", new Type[]{typeof(Boolean) , typeof(TradeInfo), typeof(TradeInfo)}),
                  scrollsTypes["TradeSystem"].Methods.GetMethod("CloseTrade")[0],
                  scrollsTypes["TradeSystem"].Methods.GetMethod("OnGUI")[0],
                  scrollsTypes["TradeSystem"].Methods.GetMethod("SetTradeRoomName", new Type[]{typeof(string)}),
                  scrollsTypes["TradeSystem"].Methods.GetMethod("Init", new Type[]{typeof(float),typeof(float),typeof(float),typeof(float),typeof(RenderTexture)}),
                 scrollsTypes["ChatUI"].Methods.GetMethod("AdjustToResolution")[0],
             };
            }
            catch
            {
                return new MethodDefinition[] { };
            }
		}


        public override bool WantsToReplace(InvocationInfo info)
        {
            if (info.target is Communicator &&  info.targetMethod.Equals("sendRequest"))
            {
                if (info.arguments[0] is RoomChatMessageMessage)
                {


                    RoomChatMessageMessage msg = (RoomChatMessageMessage)info.arguments[0];
                    string[] splitt = msg.text.Split(' ');
                    if ((splitt[0] == "/trs" || splitt[0] == "\\trs"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public override void ReplaceMethod(InvocationInfo info, out object returnValue)
        {

            returnValue = null;

            if (info.target is Communicator && info.targetMethod.Equals("sendRequest"))
            {
                //Console.WriteLine("sendrequest");
                if (info.arguments[0] is RoomChatMessageMessage)
                {


                    RoomChatMessageMessage msg = (RoomChatMessageMessage)info.arguments[0];
                    string[] splitt = msg.text.Split(' ');
                    if ((splitt[0] == "/trs" || splitt[0] == "\\trs"))
                    {
                        if (this.trading && msg.roomName == this.traderoomname)
                        {

                            Boolean donesomething = false;
                            if (splitt.Length >= 3)
                            {


                                if (splitt[1] == "search" || splitt[1] == "s")
                                {

                                    if (this.searchedself)
                                    {
                                        this.p1moddedlist.Clear();
                                        this.p1moddedlist.AddRange(this.orgicardsPlayer1);
                                        this.onlytradeableself();
                                        this.searchedself = false;
                                    }
                                    string searchstr = msg.text;
                                    searchstr = searchstr.Replace("/trs ", "");
                                    searchstr = searchstr.Replace("\\trs ", "");
                                    searchstr = searchstr.Replace("search ", "");
                                    searchstr = searchstr.Replace("s ", "");

                                    string text = "search in your cards for cards named: " + searchstr;

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);

                                    searchforname(searchstr);
                                    this.updatetrade();

                                    this.searchedself = true;

                                    donesomething = true;
                                }

                                if (splitt[1] == "searchopponent" || splitt[1] == "so")
                                {
                                    if (this.searchedoppo)
                                    {
                                        this.p2moddedlist.Clear();
                                        this.p2moddedlist.AddRange(this.orgicardsPlayer2);
                                        this.onlytradeableoppo();
                                        this.searchedoppo = false;
                                    }
                                    string searchstr = msg.text;
                                    searchstr = searchstr.Replace("/trs ", "");
                                    searchstr = searchstr.Replace("\\trs ", "");
                                    searchstr = searchstr.Replace("searchopponent ", "");
                                    searchstr = searchstr.Replace("so ", "");

                                    string text = "search in opponent cards for cards named: " + searchstr;

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);


                                    searchopponentforname(searchstr);
                                    this.updatetrade();

                                    this.searchedoppo = true;

                                    donesomething = true;
                                }


                                if (splitt[1] == "contains" || splitt[1] == "c")
                                {
                                    if (this.searchedself)
                                    {
                                        this.p1moddedlist.Clear();
                                        this.p1moddedlist.AddRange(this.orgicardsPlayer1);
                                        this.onlytradeableself();
                                        this.searchedself = false;
                                    }
                                    string searchstr = msg.text;
                                    searchstr = searchstr.Replace("/trs ", "");
                                    searchstr = searchstr.Replace("\\trs ", "");
                                    searchstr = searchstr.Replace("contains ", "");
                                    searchstr = searchstr.Replace("c ", "");
                                    string text = "search all own cards wich contains: " + searchstr;

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);

                                    containsname(searchstr);
                                    this.updatetrade();
                                    donesomething = true;
                                }
                                if (splitt[1] == "containsopponent" || splitt[1] == "co")
                                {
                                    if (this.searchedoppo)
                                    {
                                        this.p2moddedlist.Clear();
                                        this.p2moddedlist.AddRange(this.orgicardsPlayer2);
                                        this.onlytradeableoppo();
                                        this.searchedoppo = false;
                                    }
                                    string searchstr = msg.text;
                                    searchstr = searchstr.Replace("/trs ", "");
                                    searchstr = searchstr.Replace("\\trs ", "");
                                    searchstr = searchstr.Replace("containsopponent ", "");
                                    searchstr = searchstr.Replace("co ", "");
                                    string text = "search all opponent cards wich contains: " + searchstr;

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);


                                    containsopponentname(searchstr);
                                    this.updatetrade();
                                    donesomething = true;
                                }

                                if (splitt[1] == "rarity" || splitt[1] == "ra")
                                {
                                    if (this.searchedself)
                                    {
                                        this.p1moddedlist.Clear();
                                        this.p1moddedlist.AddRange(this.orgicardsPlayer1);
                                        this.onlytradeableself();
                                        this.searchedself = false;
                                    }
                                    string searchstr = msg.text;
                                    searchstr = searchstr.Replace("/trs ", "");
                                    searchstr = searchstr.Replace("\\trs ", "");
                                    searchstr = searchstr.Replace("rarity ", "");
                                    searchstr = searchstr.Replace("ra ", "");
                                    string[] raresplitt = searchstr.Split(' ');
                                    int[] rare = { -1, -1, -1 };
                                    searchstr = "";
                                    if (raresplitt.Contains("rare") || raresplitt.Contains("r")) { rare[2] = 2; searchstr = searchstr + "rare "; };
                                    if (raresplitt.Contains("uncommon") || raresplitt.Contains("u")) { rare[1] = 1; searchstr = searchstr + "uncommon "; };
                                    if (raresplitt.Contains("common") || raresplitt.Contains("c")) { rare[0] = 0; searchstr = searchstr + "common "; };
                                    string text = "search all own cards wich have the rarity: " + searchstr;

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);

                                    this.searchforownrarity(rare);
                                    this.updatetrade();
                                    donesomething = true;
                                }
                                if (splitt[1] == "rarityopponent" || splitt[1] == "rao")
                                {
                                    if (this.searchedoppo)
                                    {
                                        this.p2moddedlist.Clear();
                                        this.p2moddedlist.AddRange(this.orgicardsPlayer2);
                                        this.onlytradeableoppo();
                                        this.searchedoppo = false;
                                    }
                                    string searchstr = msg.text;
                                    searchstr = searchstr.Replace("/trs ", "");
                                    searchstr = searchstr.Replace("\\trs ", "");
                                    searchstr = searchstr.Replace("rarityopponent ", "");
                                    searchstr = searchstr.Replace("rao ", "");
                                    string[] raresplitt = searchstr.Split(' ');
                                    int[] rare = { -1, -1, -1 };
                                    searchstr = "";
                                    if (raresplitt.Contains("rare") || raresplitt.Contains("r")) { rare[2] = 2; searchstr = searchstr + "rare "; };
                                    if (raresplitt.Contains("uncommon") || raresplitt.Contains("u")) { rare[1] = 1; searchstr = searchstr + "uncommon "; };
                                    if (raresplitt.Contains("common") || raresplitt.Contains("c")) { rare[0] = 0; searchstr = searchstr + "common "; };
                                    string text = "search all opponent cards wich have the rarity: " + searchstr;

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);

                                    this.searchforopporarity(rare);
                                    this.updatetrade();
                                    donesomething = true;
                                }

                                if (splitt[1] == "resource" || splitt[1] == "re")
                                {
                                    if (this.searchedself)
                                    {
                                        this.p1moddedlist.Clear();
                                        this.p1moddedlist.AddRange(this.orgicardsPlayer1);
                                        this.onlytradeableself();
                                        this.searchedself = false;
                                    }
                                    string searchstr = msg.text;
                                    searchstr = searchstr.Replace("/trs ", "");
                                    searchstr = searchstr.Replace("\\trs ", "");
                                    searchstr = searchstr.Replace("resource ", "");
                                    searchstr = searchstr.Replace("re ", "");
                                    string[] raresplitt = searchstr.Split(' ');
                                    string[] rare = { "", "", "", "" };
                                    searchstr = "";
                                    if (raresplitt.Contains("decay") || raresplitt.Contains("d")) { rare[0] = "decay"; searchstr = searchstr + "decay "; };
                                    if (raresplitt.Contains("energy") || raresplitt.Contains("e")) { rare[1] = "energy"; searchstr = searchstr + "energy "; };
                                    if (raresplitt.Contains("growth") || raresplitt.Contains("g")) { rare[2] = "growth"; searchstr = searchstr + "growth "; };
                                    if (raresplitt.Contains("order") || raresplitt.Contains("o")) { rare[3] = "order"; searchstr = searchstr + "order "; };
                                    string text = "search own cards wich have the resource: " + searchstr;

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);

                                    this.searchforownenergy(rare);
                                    this.updatetrade();
                                    donesomething = true;
                                }

                                if (splitt[1] == "resourceopponent" || splitt[1] == "reo")
                                {
                                    if (this.searchedoppo)
                                    {
                                        this.p2moddedlist.Clear();
                                        this.p2moddedlist.AddRange(this.orgicardsPlayer2);
                                        this.onlytradeableoppo();
                                        this.searchedoppo = false;
                                    }
                                    string searchstr = msg.text;
                                    searchstr = searchstr.Replace("/trs ", "");
                                    searchstr = searchstr.Replace("\\trs ", "");
                                    searchstr = searchstr.Replace("resourceopponent ", "");
                                    searchstr = searchstr.Replace("reo ", "");
                                    string[] raresplitt = searchstr.Split(' ');
                                    string[] rare = { "", "", "", "" };
                                    searchstr = "";
                                    if (raresplitt.Contains("decay") || raresplitt.Contains("d")) { rare[0] = "decay"; searchstr = searchstr + "decay "; };
                                    if (raresplitt.Contains("energy") || raresplitt.Contains("e")) { rare[1] = "energy"; searchstr = searchstr + "energy "; };
                                    if (raresplitt.Contains("growth") || raresplitt.Contains("g")) { rare[2] = "growth"; searchstr = searchstr + "growth "; };
                                    if (raresplitt.Contains("order") || raresplitt.Contains("o")) { rare[3] = "order"; searchstr = searchstr + "order "; };
                                    string text = "search opponent cards wich have the resource: " + searchstr;

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);

                                    this.searchforoppoenergy(rare);
                                    this.updatetrade();
                                    donesomething = true;
                                }




                            }//splitt length >=3

                            if (splitt.Length == 2)
                            {
                                if (splitt[1] == "clear" || splitt[1] == "c")
                                {

                                    string text = "clear all filter";

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);
                                    this.p1moddedlist.Clear();
                                    this.p2moddedlist.Clear();
                                    this.p1moddedlist.AddRange(this.orgicardsPlayer1);
                                    this.p2moddedlist.AddRange(this.orgicardsPlayer2);
                                    this.onlytradeable();
                                    this.updatetrade();
                                    this.searchedoppo = false;
                                    this.searchedself = false;
                                    donesomething = true;
                                }
                                if (splitt[1] == "clearself" || splitt[1] == "cls")
                                {

                                    string text = "clear own filter";

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);
                                    this.p1moddedlist.Clear();
                                    this.p1moddedlist.AddRange(this.orgicardsPlayer1);
                                    this.onlytradeableself();
                                    this.updatetrade();
                                    donesomething = true;
                                    this.searchedself = false;
                                }
                                if (splitt[1] == "clearopponent" || splitt[1] == "clo")
                                {

                                    string text = "clear opponent filter";

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);

                                    this.p2moddedlist.Clear();
                                    this.p2moddedlist.AddRange(this.orgicardsPlayer2);
                                    this.onlytradeableoppo();
                                    this.updatetrade();
                                    donesomething = true;
                                    this.searchedoppo = false;
                                }
                                if (splitt[1] == "showall" || splitt[1] == "sa")
                                {

                                    string text = "displays all cards";

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);
                                    this.p1moddedlist.Clear();
                                    this.p1moddedlist.AddRange(this.orgicardsPlayer1);
                                    this.p2moddedlist.Clear();
                                    this.p2moddedlist.AddRange(this.orgicardsPlayer2);
                                    this.updatetrade();
                                    this.searchedself = false;
                                    this.searchedoppo = false;
                                    donesomething = true;
                                }

                                if (splitt[1] == "morethan3" || splitt[1] == "mt3")
                                {
                                    if (this.searchedself)
                                    {
                                        this.p1moddedlist.Clear();
                                        this.p1moddedlist.AddRange(this.orgicardsPlayer1);
                                        this.onlytradeableself();
                                        this.searchedself = false;
                                    }
                                    string text = "displays all own cards which you owns more than 3 times ";

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);


                                    this.searchmorethan3();
                                    this.updatetrade();
                                    donesomething = true;
                                }

                                if (splitt[1] == "morethan3opponent" || splitt[1] == "mt3o")
                                {
                                    if (this.searchedoppo)
                                    {
                                        this.p2moddedlist.Clear();
                                        this.p2moddedlist.AddRange(this.orgicardsPlayer2);
                                        this.onlytradeableoppo();
                                        this.searchedoppo = false;
                                    }
                                    string text = "displays all opponent cards which he owns more than 3 times ";

                                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                    App.ChatUI.handleMessage(joinmessage);
                                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);


                                    this.searchmorethan3oppo();
                                    this.updatetrade();
                                    donesomething = true;
                                }

                            } // splitt length ==2


                            if (!donesomething)
                            {
                                string text = "available commands are: search/s or searchopponent/so + Cardname\r\n contains/c or containsopponent/co + string \r\n";
                                text = text + "rarity/ra or rarityopponent/rao + rare/r and/or uncommon/u and/or common/c \r\n";
                                text = text + "resource/re or resourceopponent/reo + energy/e and/or growth/g and/or order/o and/or decay/d\r\n";
                                text = text + "clear/c, clearself/cls or clearopponent/clo, \r\n showall/sa, morethan3/mt3 or morethan3opponent/mt3o \r\n";
                                RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                                App.ChatUI.handleMessage(joinmessage);
                                App.ArenaChat.ChatRooms.ChatMessage(joinmessage);


                            }

                        }
                        else // not trading
                        {
                            string text = "/trs only works while trading in traderoom";
                            RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(msg.roomName, "<color=#777460>" + text + "</color>");
                            App.ChatUI.handleMessage(joinmessage);
                            App.ArenaChat.ChatRooms.ChatMessage(joinmessage);

                        }
                    }
                }
            }
        }

        public override void BeforeInvoke(InvocationInfo info)
        {

            return;

        }


        public override void AfterInvoke (InvocationInfo info, ref object returnValue)
        //public override bool BeforeInvoke(InvocationInfo info, out object returnValue)
        {
            
            if (info.target is ChatUI && info.targetMethod.Equals("AdjustToResolution"))//get style
            {
                this.chatLogStyle = new GUIStyle((GUIStyle)typeof(ChatUI).GetField("chatMsgStyle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target));
                
                this.chatButtonSkin = (GUISkin)typeof(ChatUI).GetField("chatButtonSkin", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);

                //Console.WriteLine("AdjustToResolution");
            }
            if (info.target is TradeSystem && info.targetMethod.Equals("Init"))//update rects
            {
                //Console.WriteLine("INIT ");
                this.menueheight = (float)Screen.width / 25.6f;
                Rect outerArea1 = (Rect)typeof(TradeSystem).GetField("outerArea1", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                Rect outerArea2 = (Rect)typeof(TradeSystem).GetField("outerArea2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                Rect innerArea = (Rect)typeof(TradeSystem).GetField("innerArea", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                Rect rectInvP1 = (Rect)typeof(TradeSystem).GetField("rectInvP1", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                Rect rectOfferP1 = (Rect)typeof(TradeSystem).GetField("rectOfferP1", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                Rect rectInvP2 = (Rect)typeof(TradeSystem).GetField("rectInvP2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                Rect rectOfferP2 = (Rect)typeof(TradeSystem).GetField("rectOfferP2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                outerArea1.height = outerArea1.height - this.menueheight;
                outerArea2.height = outerArea2.height - this.menueheight;
                innerArea.height = innerArea.height - this.menueheight;
                rectInvP1.height = rectInvP1.height - this.menueheight;
                rectOfferP1.height = rectOfferP1.height - this.menueheight;
                rectInvP2.height = rectInvP2.height - this.menueheight;
                rectOfferP2.height = rectOfferP2.height - this.menueheight;

                

                typeof(TradeSystem).GetField("outerArea1", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(info.target, outerArea1);
                typeof(TradeSystem).GetField("outerArea2", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(info.target, outerArea2);
                typeof(TradeSystem).GetField("innerArea", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(info.target, innerArea);
                typeof(TradeSystem).GetField("rectInvP1", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(info.target, rectInvP1);
                typeof(TradeSystem).GetField("rectOfferP1", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(info.target, rectOfferP1);
                typeof(TradeSystem).GetField("rectInvP2", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(info.target, rectInvP2);
                typeof(TradeSystem).GetField("rectOfferP2", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(info.target, rectOfferP2);


                CardListPopup clInventoryP1 = (CardListPopup)typeof(TradeSystem).GetField("clInventoryP1", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                CardListPopup clOfferP1 = (CardListPopup)typeof(TradeSystem).GetField("clOfferP1", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                CardListPopup clInventoryP2 = (CardListPopup)typeof(TradeSystem).GetField("clInventoryP2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                CardListPopup clOfferP2 = (CardListPopup)typeof(TradeSystem).GetField("clOfferP2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                
                
                float BOTTOM_MARGIN_EXTRA = (float)Screen.height * 0.047f;
                float num = 0.005f * (float)Screen.width;
                Vector4 margins = new Vector4(0f, 0f, 0f, 0f + BOTTOM_MARGIN_EXTRA);
                float num2 = BOTTOM_MARGIN_EXTRA - 0.01f * (float)Screen.height;

                //update clinventoryp1 (same calculation of the rect like in CardListPopup.Init(...))
                Rect outerRect = rectInvP1;
                Rect innerBGRect = new Rect(outerRect.x + margins.x, outerRect.y + margins.y, outerRect.width - (margins.x + margins.z), outerRect.height - (margins.y + margins.w));
                Rect innerRect = new Rect(innerBGRect.x + num, innerBGRect.y + num, innerBGRect.width - 2f * num, innerBGRect.height - 2f * num);
                Rect buttonLeftRect = new Rect(innerRect.x + innerRect.width * 0.03f, innerBGRect.yMax + num2 * 0.28f, innerRect.width * 0.45f, num2);
                Rect buttonRightRect = new Rect(innerRect.xMax - innerRect.width * 0.48f, innerBGRect.yMax + num2 * 0.28f, innerRect.width * 0.45f, num2);

                typeof(CardListPopup).GetField("outerRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clInventoryP1, outerRect);
                typeof(CardListPopup).GetField("innerBGRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clInventoryP1, innerBGRect);
                typeof(CardListPopup).GetField("innerRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clInventoryP1, innerRect);
                typeof(CardListPopup).GetField("buttonLeftRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clInventoryP1, buttonLeftRect);
                typeof(CardListPopup).GetField("buttonRightRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clInventoryP1, buttonRightRect);

                //update clinventoryp2
                outerRect = rectInvP2;
                innerBGRect = new Rect(outerRect.x + margins.x, outerRect.y + margins.y, outerRect.width - (margins.x + margins.z), outerRect.height - (margins.y + margins.w));
                innerRect = new Rect(innerBGRect.x + num, innerBGRect.y + num, innerBGRect.width - 2f * num, innerBGRect.height - 2f * num);
                buttonLeftRect = new Rect(innerRect.x + innerRect.width * 0.03f, innerBGRect.yMax + num2 * 0.28f, innerRect.width * 0.45f, num2);
                buttonRightRect = new Rect(innerRect.xMax - innerRect.width * 0.48f, innerBGRect.yMax + num2 * 0.28f, innerRect.width * 0.45f, num2);

                typeof(CardListPopup).GetField("outerRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clInventoryP2, outerRect);
                typeof(CardListPopup).GetField("innerBGRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clInventoryP2, innerBGRect);
                typeof(CardListPopup).GetField("innerRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clInventoryP2, innerRect);
                typeof(CardListPopup).GetField("buttonLeftRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clInventoryP2, buttonLeftRect);
                typeof(CardListPopup).GetField("buttonRightRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clInventoryP2, buttonRightRect);

                //update clOfferP1
                outerRect = rectOfferP1;
                innerBGRect = new Rect(outerRect.x + margins.x, outerRect.y + margins.y, outerRect.width - (margins.x + margins.z), outerRect.height - (margins.y + margins.w));
                innerRect = new Rect(innerBGRect.x + num, innerBGRect.y + num, innerBGRect.width - 2f * num, innerBGRect.height - 2f * num);
                buttonLeftRect = new Rect(innerRect.x + innerRect.width * 0.03f, innerBGRect.yMax + num2 * 0.28f, innerRect.width * 0.45f, num2);
                buttonRightRect = new Rect(innerRect.xMax - innerRect.width * 0.48f, innerBGRect.yMax + num2 * 0.28f, innerRect.width * 0.45f, num2);

                typeof(CardListPopup).GetField("outerRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clOfferP1, outerRect);
                typeof(CardListPopup).GetField("innerBGRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clOfferP1, innerBGRect);
                typeof(CardListPopup).GetField("innerRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clOfferP1, innerRect);
                typeof(CardListPopup).GetField("buttonLeftRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clOfferP1, buttonLeftRect);
                typeof(CardListPopup).GetField("buttonRightRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clOfferP1, buttonRightRect);

                //update clOfferP2
                outerRect = rectOfferP2;
                innerBGRect = new Rect(outerRect.x + margins.x, outerRect.y + margins.y, outerRect.width - (margins.x + margins.z), outerRect.height - (margins.y + margins.w));
                innerRect = new Rect(innerBGRect.x + num, innerBGRect.y + num, innerBGRect.width - 2f * num, innerBGRect.height - 2f * num);
                buttonLeftRect = new Rect(innerRect.x + innerRect.width * 0.03f, innerBGRect.yMax + num2 * 0.28f, innerRect.width * 0.45f, num2);
                buttonRightRect = new Rect(innerRect.xMax - innerRect.width * 0.48f, innerBGRect.yMax + num2 * 0.28f, innerRect.width * 0.45f, num2);

                typeof(CardListPopup).GetField("outerRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clOfferP2, outerRect);
                typeof(CardListPopup).GetField("innerBGRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clOfferP2, innerBGRect);
                typeof(CardListPopup).GetField("innerRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clOfferP2, innerRect);
                typeof(CardListPopup).GetField("buttonLeftRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clOfferP2, buttonLeftRect);
                typeof(CardListPopup).GetField("buttonRightRect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clOfferP2, buttonRightRect);



                this.p1rectsearchmenu = new Rect(outerArea1.x, outerArea1.y + outerArea1.height, outerArea1.width, this.menueheight);
                this.p2rectsearchmenu = new Rect(outerArea2.x, outerArea2.y + outerArea2.height, outerArea2.width, this.menueheight);

                this.p1searchrect = new Rect(p1rectsearchmenu.x + 5*num, p1rectsearchmenu.y + num+2, outerArea1.width/3, p1rectsearchmenu.height -  2f*num-4);
                this.p2searchrect = new Rect(p2rectsearchmenu.x + 5*num, p2rectsearchmenu.y + num+2, outerArea2.width / 3, p2rectsearchmenu.height - 2f*num-4);

                this.outerFrame1 = new ScrollsFrame(this.p1rectsearchmenu).AddNinePatch(ScrollsFrame.Border.DARK_CURVED, NinePatch.Patches.TOP | NinePatch.Patches.TOP_RIGHT | NinePatch.Patches.CENTER | NinePatch.Patches.RIGHT | NinePatch.Patches.BOTTOM | NinePatch.Patches.BOTTOM_RIGHT).AddNinePatch(ScrollsFrame.Border.DARK_SHARP, NinePatch.Patches.TOP_LEFT | NinePatch.Patches.LEFT | NinePatch.Patches.CENTER | NinePatch.Patches.BOTTOM_LEFT);
                this.outerFrame2 = new ScrollsFrame(this.p2rectsearchmenu).AddNinePatch(ScrollsFrame.Border.DARK_CURVED, NinePatch.Patches.TOP_LEFT | NinePatch.Patches.TOP | NinePatch.Patches.LEFT | NinePatch.Patches.CENTER | NinePatch.Patches.BOTTOM_LEFT | NinePatch.Patches.BOTTOM).AddNinePatch(ScrollsFrame.Border.DARK_SHARP, NinePatch.Patches.TOP_RIGHT | NinePatch.Patches.CENTER | NinePatch.Patches.RIGHT | NinePatch.Patches.BOTTOM_RIGHT);


                this.p1growthrect = new Rect(p1searchrect.x + p1searchrect.width + 3, p1rectsearchmenu.y + num + 2, p1rectsearchmenu.height - 2f * num - 4, p1rectsearchmenu.height - 2f * num - 4);
                this.p1orderrect = new Rect(p1growthrect.x + p1growthrect.width + 3, p1rectsearchmenu.y + num + 2, p1rectsearchmenu.height - 2f * num - 4, p1rectsearchmenu.height - 2f * num - 4);
                this.p1energyrect = new Rect(p1orderrect.x + p1orderrect.width + 3, p1rectsearchmenu.y + num + 2, p1rectsearchmenu.height - 2f * num - 4, p1rectsearchmenu.height - 2f * num - 4);
                this.p1decayrect = new Rect(p1energyrect.x + p1energyrect.width + 3, p1rectsearchmenu.y + num + 2, p1rectsearchmenu.height - 2f * num - 4, p1rectsearchmenu.height - 2f * num - 4);
                this.p1commonrect = new Rect(p1decayrect.x + p1decayrect.width + 3, p1rectsearchmenu.y + num + 2, p1rectsearchmenu.height - 2f * num - 4, p1rectsearchmenu.height - 2f * num - 4);
                this.p1uncommonrect = new Rect(p1commonrect.x + p1commonrect.width + 3, p1rectsearchmenu.y + num + 2, p1rectsearchmenu.height - 2f * num - 4, p1rectsearchmenu.height - 2f * num - 4);
                this.p1rarerect = new Rect(p1uncommonrect.x + p1uncommonrect.width + 3, p1rectsearchmenu.y + num + 2, p1rectsearchmenu.height - 2f * num - 4, p1rectsearchmenu.height - 2f * num - 4);
                this.p1mt3rect = new Rect(p1rarerect.x + p1rarerect.width + 3, p1rectsearchmenu.y + num + 2, p1rectsearchmenu.height - 2f * num - 4, p1rectsearchmenu.height - 2f * num - 4);
                this.p1clearrect = new Rect(p1rectsearchmenu.x + p1rectsearchmenu.width - p1rectsearchmenu.height-num, p1rectsearchmenu.y + num + 2, p1rectsearchmenu.height - 2f * num - 4, p1rectsearchmenu.height - 2f * num - 4);

                this.p2growthrect = new Rect(p2searchrect.x + p2searchrect.width + 3, p2rectsearchmenu.y + num + 2, p2rectsearchmenu.height - 2f * num - 4, p2rectsearchmenu.height - 2f * num - 4);
                this.p2orderrect = new Rect(p2growthrect.x + p2growthrect.width + 3, p2rectsearchmenu.y + num + 2, p2rectsearchmenu.height - 2f * num - 4, p2rectsearchmenu.height - 2f * num - 4);
                this.p2energyrect = new Rect(p2orderrect.x + p2orderrect.width + 3, p2rectsearchmenu.y + num + 2, p2rectsearchmenu.height - 2f * num - 4, p2rectsearchmenu.height - 2f * num - 4);
                this.p2decayrect = new Rect(p2energyrect.x + p2energyrect.width + 3, p2rectsearchmenu.y + num + 2, p2rectsearchmenu.height - 2f * num - 4, p2rectsearchmenu.height - 2f * num - 4);
                this.p2commonrect = new Rect(p2decayrect.x + p2decayrect.width + 3, p2rectsearchmenu.y + num + 2, p2rectsearchmenu.height - 2f * num - 4, p2rectsearchmenu.height - 2f * num - 4);
                this.p2uncommonrect = new Rect(p2commonrect.x + p2commonrect.width + 3, p2rectsearchmenu.y + num + 2, p2rectsearchmenu.height - 2f * num - 4, p2rectsearchmenu.height - 2f * num - 4);
                this.p2rarerect = new Rect(p2uncommonrect.x + p2uncommonrect.width + 3, p2rectsearchmenu.y + num + 2, p2rectsearchmenu.height - 2f * num - 4, p2rectsearchmenu.height - 2f * num - 4);
                this.p2mt3rect = new Rect(p2rarerect.x + p2rarerect.width + 3, p2rectsearchmenu.y + num + 2, p2rectsearchmenu.height - 2f * num - 4, p2rectsearchmenu.height - 2f * num - 4);
                this.p2clearrect = new Rect(p2rectsearchmenu.x + p2rectsearchmenu.width - p2rectsearchmenu.height-num, p2rectsearchmenu.y + num + 2, p2rectsearchmenu.height - 2f * num - 4, p2rectsearchmenu.height - 2f * num - 4);
                


            }

            if (info.target is TradeSystem && info.targetMethod.Equals("OnGUI"))//draw menu
            {
                //int state = (int)typeof(TradeSystem).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                if (trading)
                {
                    outerFrame1.Draw();
                    outerFrame2.Draw();
                    Color dblack = new Color(1f, 1f, 1f, 0.2f);
                    GUI.color = new Color(1f, 1f, 1f, 0.6f);
                    GUI.skin = this.chatButtonSkin;
                    GUI.Box(this.p1searchrect, string.Empty);
                    GUI.Box(this.p2searchrect, string.Empty);
                    GUI.color = Color.white;
                    string selfcopy = this.selfsearchstring;
                    string oppocopy = this.opposearchstring;
                    this.selfsearchstring = GUI.TextField(this.p1searchrect, this.selfsearchstring, this.chatLogStyle);
                    this.opposearchstring = GUI.TextField(this.p2searchrect, this.opposearchstring, this.chatLogStyle);

                    GUI.contentColor = Color.white;
                    GUI.color = Color.white;
                    if (!p1growthbool) { GUI.color = dblack; }
                    bool p1growthclick = GUI.Button(p1growthrect, growthres);
                    GUI.color = Color.white;
                    if (!p1orderbool) { GUI.color = dblack; }
                    bool p1orderclick = GUI.Button(p1orderrect, orderres);
                    GUI.color = Color.white;
                    if (!p1energybool) { GUI.color = dblack; }
                    bool p1energyclick = GUI.Button(p1energyrect, energyres);
                    GUI.color = Color.white;
                    if (!p1decaybool) { GUI.color = dblack; }
                    bool p1decayclick = GUI.Button(p1decayrect, decayres);
                    GUI.color = Color.white;
                    if (!p1commonbool) { GUI.color = dblack; }
                    GUI.contentColor = Color.gray;
                    bool p1commonclick = GUI.Button(p1commonrect,"C");
                    GUI.color = Color.white;
                    if (!p1uncommonbool) { GUI.color = dblack; }
                    GUI.contentColor = Color.white;
                    bool p1uncommonclick = GUI.Button(p1uncommonrect, "U");
                    GUI.color = Color.white;
                    if (!p1rarebool) { GUI.color = dblack; }
                    GUI.contentColor = Color.yellow;
                    bool p1rareclick = GUI.Button(p1rarerect, "R");
                    GUI.contentColor = Color.white;
                    GUI.color = Color.white;
                    if (!p1mt3bool) { GUI.color = dblack; }
                    bool p1mt3click = GUI.Button(p1mt3rect, ">3");
                    GUI.color = Color.white;
                    GUI.contentColor = Color.red;
                    bool p1closeclick = GUI.Button(p1clearrect, "X");

                    if (p1growthclick) { p1growthbool = !p1growthbool; };
                    if (p1orderclick) { p1orderbool = !p1orderbool; }
                    if (p1energyclick) { p1energybool = !p1energybool; };
                    if (p1decayclick) { p1decaybool = !p1decaybool; }
                    if (p1commonclick) { p1commonbool = !p1commonbool; };
                    if (p1uncommonclick) { p1uncommonbool = !p1uncommonbool; }
                    if (p1rareclick) { p1rarebool = !p1rarebool; };
                    if (p1mt3click) { p1mt3bool = !p1mt3bool; }
                    if (p1closeclick) 
                    {
                        this.selfsearchstring = "";
                        p1growthbool = true;
                        p1orderbool = true;
                        p1energybool = true;
                        p1decaybool = true;
                        p1commonbool = true;
                        p1uncommonbool = true;
                        p1rarebool = true;
                        p1mt3bool = false;
                    }

                    //clear p1moddedlist only if necessary
                    if (selfcopy.Length > this.selfsearchstring.Length || p1closeclick || (p1growthclick && p1growthbool) || (p1orderclick && p1orderbool) || (p1energyclick && p1energybool) || (p1decayclick && p1decaybool) || (p1commonclick && p1commonbool) || (p1uncommonclick && p1uncommonbool) || (p1rareclick && p1rarebool) || p1mt3click)
                    {
                        //Console.WriteLine("delete dings####");
                        this.p1moddedlist.Clear();
                        this.p1moddedlist.AddRange(this.orgicardsPlayer1);
                        string[] res = { "", "", "", "" };
                        if (p1decaybool) { res[0] = "decay"; };
                        if (p1energybool) { res[1] = "energy"; };
                        if (p1growthbool) { res[2] = "growth"; };
                        if (p1orderbool) { res[3] = "order"; };
                        int[] rare = { -1, -1, -1 };
                        if (p1rarebool) { rare[2] = 2; };
                        if (p1uncommonbool) { rare[1] = 1; };
                        if (p1commonbool) { rare[0] = 0; };
                        if (this.p1mt3bool)
                        {
                            this.searchmorethan3();
                        }
                        this.onlytradeableself();
                        if (this.selfsearchstring != "")
                        {
                            this.containsname(this.selfsearchstring);
                        }
                        this.searchforownenergy(res);
                        this.searchforownrarity(rare);
                        this.updatetradeself();

                    }
                    else
                    {

                        if (selfcopy != this.selfsearchstring )
                        {

                            if (this.selfsearchstring != "")
                            {
                                this.containsname(this.selfsearchstring);
                                this.updatetradeself();
                            }
                            

                        }
                        if (p1growthclick || p1orderclick || p1energyclick || p1decayclick )
                        {
                            string[] res = { "", "", "", "" };
                            if (p1decaybool) { res[0] = "decay"; };
                            if (p1energybool) { res[1] = "energy"; };
                            if (p1growthbool) { res[2] = "growth"; };
                            if (p1orderbool) { res[3] = "order"; };
                            this.searchforownenergy(res);
                            this.updatetradeself();

                        }
                        if (p1commonclick || p1uncommonclick || p1rareclick)
                        {

                            int[] rare = { -1, -1, -1 };
                            if (p1rarebool) { rare[2] = 2; };
                            if (p1uncommonbool) { rare[1] = 1; };
                            if (p1commonbool) { rare[0] = 0; };
                            this.searchforownrarity(rare);
                            this.updatetradeself();
                        }
                        
                    }

                    GUI.contentColor = Color.white;
                    GUI.color = Color.white;
                    if (!p2growthbool) { GUI.color = dblack; }
                    bool p2growthclick = GUI.Button(p2growthrect, growthres );
                    GUI.color = Color.white;
                    if (!p2orderbool) { GUI.color = dblack; }
                    bool p2orderclick = GUI.Button(p2orderrect, orderres);
                    GUI.color = Color.white;
                    if (!p2energybool) { GUI.color = dblack; }
                    bool p2energyclick = GUI.Button(p2energyrect, energyres);
                    GUI.color = Color.white;
                    if (!p2decaybool) { GUI.color = dblack; }
                    bool p2decayclick = GUI.Button(p2decayrect, decayres);
                    GUI.color = Color.white;
                    if (!p2commonbool) { GUI.color = dblack; }
                    GUI.contentColor = Color.gray;
                    bool p2commonclick = GUI.Button(p2commonrect, "C");
                    GUI.color = Color.white;
                    if (!p2uncommonbool) { GUI.color = dblack; }
                    GUI.contentColor = Color.white;
                    bool p2uncommonclick = GUI.Button(p2uncommonrect, "U");
                    GUI.color = Color.white;
                    if (!p2rarebool) { GUI.color = dblack; }
                    GUI.contentColor = Color.yellow;
                    bool p2rareclick = GUI.Button(p2rarerect, "R");
                    GUI.contentColor = Color.white;
                    GUI.color = Color.white;
                    if (!p2mt3bool) { GUI.color = dblack; }
                    bool p2mt3click = GUI.Button(p2mt3rect, ">3");
                    GUI.color = Color.white;
                    GUI.contentColor = Color.red;
                    bool p2closeclick = GUI.Button(p2clearrect, "X");

                    if (p2growthclick) { p2growthbool = !p2growthbool; };
                    if (p2orderclick) { p2orderbool = !p2orderbool; }
                    if (p2energyclick) { p2energybool = !p2energybool; };
                    if (p2decayclick) { p2decaybool = !p2decaybool; }
                    if (p2commonclick) { p2commonbool = !p2commonbool; };
                    if (p2uncommonclick) { p2uncommonbool = !p2uncommonbool; }
                    if (p2rareclick) { p2rarebool = !p2rarebool; };
                    if (p2mt3click) { p2mt3bool = !p2mt3bool; }
                    if (p2closeclick)
                    {
                        this.opposearchstring = "";
                        p2growthbool = true;
                        p2orderbool = true;
                        p2energybool = true;
                        p2decaybool = true;
                        p2commonbool = true;
                        p2uncommonbool = true;
                        p2rarebool = true;
                        p2mt3bool = false;
                    }

                    //clear p1moddedlist only if necessary
                    if (oppocopy.Length > this.opposearchstring.Length || p2closeclick || (p2growthclick && p2growthbool) || (p2orderclick && p2orderbool) || (p2energyclick && p2energybool) || (p2decayclick && p2decaybool) || (p2commonclick && p2commonbool) || (p2uncommonclick && p2uncommonbool) || (p2rareclick && p2rarebool) || p2mt3click)
                    {
                        //Console.WriteLine("delete dings####");
                        this.p2moddedlist.Clear();
                        this.p2moddedlist.AddRange(this.orgicardsPlayer2);

                        string[] res = { "", "", "", "" };
                        if (p2decaybool) { res[0] = "decay"; };
                        if (p2energybool) { res[1] = "energy"; };
                        if (p2growthbool) { res[2] = "growth"; };
                        if (p2orderbool) { res[3] = "order"; };
                        int[] rare = { -1, -1, -1 };
                        if (p2rarebool) { rare[2] = 2; };
                        if (p2uncommonbool) { rare[1] = 1; };
                        if (p2commonbool) { rare[0] = 0; };

                        this.onlytradeableoppo();
                        if (this.opposearchstring != "")
                        {
                            this.containsopponentname(this.opposearchstring);
                        }
                        this.searchforoppoenergy(res);
                        this.searchforopporarity(rare);
                        if (this.p2mt3bool)
                        {
                            this.searchmorethan3oppo();
                        }

                        this.updatetradeoppo();

                    }
                    else
                    {

                        if (oppocopy != this.opposearchstring)
                        {

                            if (this.opposearchstring != "")
                            {
                                this.containsopponentname(this.opposearchstring);
                                this.updatetradeoppo();
                            }


                        }
                        if (p2growthclick || p2orderclick || p2energyclick || p2decayclick)
                        {
                            string[] res = { "", "", "", "" };
                            if (p2decaybool) { res[0] = "decay"; };
                            if (p2energybool) { res[1] = "energy"; };
                            if (p2growthbool) { res[2] = "growth"; };
                            if (p2orderbool) { res[3] = "order"; };
                            this.searchforoppoenergy(res);
                            this.updatetradeoppo();

                        }
                        if (p2commonclick || p2uncommonclick || p2rareclick)
                        {

                            int[] rare = { -1, -1, -1 };
                            if (p2rarebool) { rare[2] = 2; };
                            if (p2uncommonbool) { rare[1] = 1; };
                            if (p2commonbool) { rare[0] = 0; };
                            this.searchforopporarity(rare);
                            this.updatetradeoppo();
                        }

                    }

                    /*
                    if (oppocopy != this.opposearchstring || p2growthclick || p2orderclick || p2energyclick || p2decayclick || p2commonclick || p2uncommonclick || p2rareclick || p2mt3click || p2closeclick)
                    {
                        this.p2moddedlist.Clear();
                        this.p2moddedlist.AddRange(this.orgicardsPlayer2);

                        string[] res = { "", "", "", "" };
                        if (p2decaybool) { res[0] = "decay"; };
                        if (p2energybool) { res[1] = "energy"; };
                        if (p2growthbool) { res[2] = "growth"; };
                        if (p2orderbool) { res[3] = "order"; };
                        int[] rare = { -1, -1, -1 };
                        if (p2rarebool) { rare[2] = 2; };
                        if (p2uncommonbool) { rare[1] = 1; };
                        if (p2commonbool) { rare[0] = 0; };

                        this.onlytradeableoppo();
                        if (this.opposearchstring != "")
                        {
                            this.containsopponentname(this.opposearchstring);
                        }
                        this.searchforoppoenergy(res);
                        this.searchforopporarity(rare);
                        if (this.p2mt3bool)
                        {
                            this.searchmorethan3oppo();
                        }

                        this.updatetrade();

                    }*/
                }
            }

            if (info.target is Lobby && info.targetMethod.Equals("Start"))
            {
                //Console.WriteLine("startlobby");
                if (lby == null)
                {
                    lby = (Lobby)info.target;
                    lbyinfo = info;
                    this.p1cards = (List<Card>)typeof(Lobby).GetField("cardsPlayer1", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(lbyinfo.target);
                    this.p2cards = (List<Card>)typeof(Lobby).GetField("cardsPlayer2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(lbyinfo.target);
                    anzupdates = 0;
                    this.searchedself = false;
                    this.searchedoppo = false;

                }


            }

            if (info.target is TradeSystem && info.targetMethod.Equals("SetTradeRoomName"))
            {
                this.traderoomname = (string)info.arguments[0];


            }


            if (info.target is TradeSystem && info.targetMethod.Equals("StartTrade"))
            {
                //Console.WriteLine("STARTTRAD2E");
                if (ts == null)
                {
                    trading = true;
                    ts = (TradeSystem)info.target;
                    this.p1moddedlist.Clear();
                    this.p2moddedlist.Clear();
                    this.orginalp1name = (string)info.arguments[2];
                    this.orginalp2name = (string)info.arguments[3];
                    this.orginalint = (int)info.arguments[4];
                    

                }


            }

            if (info.target is TradeSystem && info.targetMethod.Equals("UpdateView"))
            {
                //Console.WriteLine("updateview"+anzupdates.ToString());
                if ((Boolean)info.arguments[0] == false && this.anzupdates<=1)
                {
                    

                    this.orgicardsPlayer1.Clear();
                    this.orgicardsPlayer2.Clear();
                    this.orgicardsPlayer1.AddRange(p1cards);
                    this.orgicardsPlayer2.AddRange(p2cards);
                    this.p1moddedlist.Clear();
                    this.p2moddedlist.Clear();
                    this.p1moddedlist.AddRange(this.orgicardsPlayer1);
                    this.p2moddedlist.AddRange(this.orgicardsPlayer2);
                    
                    //Console.WriteLine("cards");
                    //foreach (Card c in this.orgicardsPlayer1) { Console.WriteLine(c.getName()); };
                    //foreach (Card c in this.orgicardsPlayer2) { Console.WriteLine(c.getName()); };


                    //Rect outerArea1 = (Rect)typeof(Lobby).GetField("outerArea1", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(lbyinfo.target);

                    //Console.WriteLine("outer area" + outerArea1.height);


                    if (anzupdates == 1) // only show at the second time (first time you get own cards, second time you get opponent cards)
                    {
                        this.onlytradeable();
                        this.updatetrade();
                    }
                    
                }
                anzupdates++;
                //collect tradeview changes:
                if ((Boolean)info.arguments[0] == true)
                {
                    this.player1 = (TradeInfo)info.arguments[1];
                    this.player2 = (TradeInfo)info.arguments[2];
                    this.updatetrade();
                }
            }

            if (info.target is TradeSystem && info.targetMethod.Equals("CloseTrade"))
            {
                ts = null;
                lby = null;
                player1 = new TradeInfo();
                player2 = new TradeInfo();
                trading = false;
                this.traderoomname = "";

            }




            return;
        }

        
	}
}

