using System;
using Xamarin.Forms;
using SQLForms.SQL;
using System.Linq;
using System.Text;

namespace SQLForms
{
    public class SQLExample : ContentPage
    {
        string name, details, address, postcode;
        Label resultLabel;

        public SQLExample()
        {
            var editName = new Entry(){ Placeholder = "Event name" };
            editName.TextChanged += delegate
            {
                name = editName.Text;
            };
            var editDetails = new Entry() { Placeholder = "Event details" };
            editDetails.TextChanged += delegate
            {
                details = editDetails.Text;
            };
            var editAddress = new Entry(){ Placeholder = "Event address" };
            editAddress.TextChanged += delegate
            {
                address = editAddress.Text;
            };
            var editPostcode = new Entry(){ Placeholder = "Event postocode" };
            editPostcode.TextChanged += delegate
            {
                postcode = editPostcode.Text;
            };

            var btnEnterData = new Button(){ Text = "Submit to database" };
            btnEnterData.Clicked += EnterData;

            resultLabel = new Label(){ Text = "" };
            var btnSearchData = new Button() { Text = "Search on parameter" };
            btnSearchData.Clicked += SearchData;

            Content = new StackLayout()
            {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(20),
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Children =
                {
                    new StackLayout()
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            new Label(){ Text = "Event name" }, editName
                        }
                    },
                    new StackLayout()
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            new Label(){ Text = "Details" }, editDetails
                        }
                    },
                    new StackLayout()
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            new Label(){ Text = "Address" }, editAddress
                        }
                    },
                    new StackLayout()
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            new Label(){ Text = "Postcode" }, editPostcode
                        }
                    },
                    btnEnterData,
                    btnSearchData,
                    new StackLayout()
                    {
                        Padding = new Thickness(0, 10, 0, 0),
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            new Label(){ Text = "Search results" }, resultLabel
                        }
                    }
                }
            };
        }

        async void EnterData(object s, EventArgs e)
        {
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(address) && string.IsNullOrEmpty(postcode) && string.IsNullOrEmpty(details))
            {
                await DisplayAlert("Enter data", "You have not entered any data", "OK");
                return;
            }
            var ev = new Event()
            {
                event_name = name,
                event_address = address,
                event_postcode = postcode,
                event_details = details,
                __updatedAt = DateTime.Now
            };
            App.Singleton.DBManager.AddOrUpdateEvent(ev);
        }

        async void SearchData(object s, EventArgs e)
        {
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(address) && string.IsNullOrEmpty(postcode) && string.IsNullOrEmpty(details))
            {
                await DisplayAlert("Search data", "You have not entered anything to search for", "OK");
                return;
            }
            var res = App.Singleton.DBManager.GetListOfObjects<Event>().Where(t => t.event_name == name).Where(t => t.event_address == address).Where(t => t.event_details == details).Where(t => t.event_postcode == postcode).OrderBy(t => t.event_name).ToList();
            if (res.Count == 0)
                resultLabel.Text = "Nothing returned for your search";
            else
            {
                var results = new StringBuilder();
                foreach (var r in res)
                    results.Append(string.Format("Event name : {0}\nAdress : {1}\nPostcode : {2}\nDetails : {3}\n", r.event_name, r.event_address, r.event_postcode, r.event_details));
                resultLabel.Text = results.ToString();
            }
        }
    }
}

