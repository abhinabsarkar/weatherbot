using Microsoft.Bot.Connector;
using System.Collections.Generic;

namespace WeatherBot
{
    public class OutputFormatter
    {
        public static List<ThumbnailCard> createCards(List<LocationResult> locations)
        {
            List<ThumbnailCard> cards = new List<ThumbnailCard>();

            foreach (LocationResult l in locations)
            {
                List<CardAction> btns = new List<CardAction>();
                btns.Add(new CardAction() { Type = "postBack", Title = l.city + "," + l.state + "," + l.country_name, Value = l.city + "," + l.state + "," + l.country_name });
                cards.Add(new ThumbnailCard()
                {
                    Buttons = btns

                });
            }

            return cards;
        }
    }
}