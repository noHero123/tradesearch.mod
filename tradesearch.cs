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
                    Console.WriteLine(card.getName());
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
                    Console.WriteLine(card.getName());
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
                  scrollsTypes["TradeSystem"].Methods.GetMethod("SetTradeRoomName", new Type[]{typeof(string)}),
             };
            }
            catch
            {
                return new MethodDefinition[] { };
            }
		}

        public override bool BeforeInvoke(InvocationInfo info, out object returnValue)
        {

            returnValue = null;

            if (info.targetMethod.Equals("sendRequest"))
            {
                Console.WriteLine("sendrequest");
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

                        return true;
                    }
                }


                

            }

           


            return false;
        }

        public override void AfterInvoke (InvocationInfo info, ref object returnValue)
        //public override bool BeforeInvoke(InvocationInfo info, out object returnValue)
        {

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
                Console.WriteLine("updateview"+anzupdates.ToString());
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
                    
                    Console.WriteLine("cards");
                    foreach (Card c in this.orgicardsPlayer1) { Console.WriteLine(c.getName()); };
                    foreach (Card c in this.orgicardsPlayer2) { Console.WriteLine(c.getName()); };
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

