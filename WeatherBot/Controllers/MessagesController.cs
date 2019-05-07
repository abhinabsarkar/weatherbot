using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.ApplicationInsights;
using System.Diagnostics;
using System.Collections.Generic;

namespace WeatherBot
{

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        StateClient botState;
        BotData botData;
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            //Declare variables
            WeatherAPIModel weatherAPIModel;
            WeatherAPIModelNew weatherAPIModelNew;
            WeatherResponse weatherResponse;

            //Add application insights telemetry
            var telemetry = new TelemetryClient();

            //Connector client for sending and receiving activities via the communication channel (fb messenger, skype, etc)
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            //Message reply object
            Activity reply = activity.CreateReply();

            botState = activity.GetStateClient();
            botData = await botState.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

            //Fetch user intent and entity via LUIS
            string weatherDetails;

            if (activity.Type == ActivityTypes.Message)
            {
                try
                {
                    //TODO: Does getting the geolocation work ?
                    //Get Lattitude and Longitude values from Message                    
                    //TestCoordinates(activity.Entities);

                    weatherResponse = new WeatherResponse();

                    //Check if user has selected one of the multiple locations
                    if (botData.GetProperty<bool>("LocationSelected"))
                    {
                        string country = null;
                        string state = null;
                        string city = null;
                        try
                        {
                            country = activity.Text.Split(',')[2];
                            state = activity.Text.Split(',')[1];
                            city = activity.Text.Split(',')[0];
                        }
                        catch (Exception ex)
                        {
                            botData.SetProperty<bool>("LocationSelected", false);
                            telemetry.TrackTrace("Error in split" + ex.StackTrace); //Sends stack traces
                            botState.BotState.DeleteStateForUser(activity.ChannelId, activity.From.Id);
                            throw ex;
                        }

                        //Check if country is USA
                        if (country.ToUpper() == "USA")
                        {
                            //if country is USA, call GetWeatherDetailsUSA
                            weatherAPIModel = await Weather.GetWeatherDetailsUSA(city, state);
                            weatherDetails = weatherResponse.FormattedResponse(weatherAPIModel);

                            //delete user data
                            botData.SetProperty<bool>("LocationSelected", false);
                            botData = await botState.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, botData);

                            // return reply to the user
                            reply = activity.CreateReply(weatherDetails);
                            await connector.Conversations.ReplyToActivityAsync(reply);
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            //if country is not USA, call GetWeatherDetailsOthers
                            weatherAPIModel = await Weather.GetWeatherDetailsOthers(city, country);
                            weatherDetails = weatherResponse.FormattedResponse(weatherAPIModel);

                            //delete user data
                            botData.SetProperty<bool>("LocationSelected", false);
                            botData = await botState.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, botData);

                            // return reply to the user
                            reply = activity.CreateReply(weatherDetails);
                            await connector.Conversations.ReplyToActivityAsync(reply);
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                    }

                    WeatherLUISModel weatherModelLUIS = await WeatherEntityLUIS.GetEntityFromLUIS(activity.Text);

                    //Get weather details for the Entity
                    switch (weatherModelLUIS.intents[0].intent)
                    {
                        case "WeatherDetails":
                            weatherAPIModelNew = await Weather.GetWeatherDetails(weatherModelLUIS.entities[0].entity);
                            if (weatherAPIModelNew != null)
                            {
                                weatherDetails = weatherResponse.FormattedResponse(weatherAPIModelNew);
                            }
                            else
                            {
                                weatherDetails = "Sorry, weather details for this city is not available.";
                                telemetry.TrackEvent("CityNotFound");
                            }
                            break;
                        //case "WeatherDetails":
                        //    Object[] multiDataType = await Weather.GetWeatherDetails(weatherModelLUIS.entities[0].entity);
                        //    weatherAPIModel = (WeatherAPIModel)multiDataType[0];
                        //    if (weatherAPIModel.current_observation != null)
                        //    {
                        //        weatherDetails = weatherResponse.FormattedResponse(weatherAPIModel);
                        //        break;
                        //    }
                        //    else
                        //    {
                        //        if (weatherAPIModel.response.error != null)
                        //        {
                        //            weatherDetails = weatherAPIModel.response.error.description;
                        //            break;
                        //        }
                        //        else
                        //        {
                        //            LocationAPIModel multipleLocations = (LocationAPIModel)multiDataType[1];
                        //            weatherDetails = "There are multiple locations named " + weatherModelLUIS.entities[0].entity + ". Please select one.";
                        //            reply = activity.CreateReply(weatherDetails);
                        //            reply.Attachments = new List<Attachment>();
                        //            List<ThumbnailCard> cards;
                        //            List<LocationResult> locations = new List<LocationResult>(multipleLocations.response.results);

                        //            //Prepare cards for multiple locations
                        //            cards = OutputFormatter.createCards(locations);

                        //            foreach (ThumbnailCard c in cards)
                        //                reply.Attachments.Add(c.ToAttachment());

                        //            reply.AttachmentLayout = AttachmentLayoutTypes.List;

                        //            try
                        //            {
                        //                botData.SetProperty<bool>("LocationSelected", true);
                        //                await botState.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, botData);
                        //            }
                        //            catch { }

                        //            await connector.Conversations.ReplyToActivityAsync(reply);
                        //            return Request.CreateResponse(HttpStatusCode.OK);
                        //        }
                        //    }

                        case "Greeting":
                            weatherDetails = "Hello! I am weather bot. You can ask me about the weather of any city";
                            telemetry.TrackEvent("Greetings");
                            break;

                        case "Question":
                            weatherDetails = "I can help you with current weather conditions. Try me";
                            telemetry.TrackEvent("Question");
                            break;

                        case "Acknowledge":
                            weatherDetails = "Glad to talk to you";
                            telemetry.TrackEvent("Acknowledge");
                            break;

                        default:
                            weatherDetails = "Sorry, I am not able to undrestand you.";
                            telemetry.TrackEvent("Ignorant");
                            break;
                    }

                    // return reply to the user
                    reply = activity.CreateReply(weatherDetails);
                    await connector.Conversations.ReplyToActivityAsync(reply);

                }
                catch (Exception ex)
                {
                    Trace.TraceError("Exception occured " + ex.ToString());
                    botData.SetProperty<bool>("LocationSelected", false);
                    weatherDetails = "Sorry I am not able to undrestand you.";
                    reply = activity.CreateReply(weatherDetails);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}