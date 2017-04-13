To set up the project to run update the following:

ElasticSearch:
-Go to the homecontroller and set the node Uri to the IP address of your ElasticSearch Server (line 54)
-Do the same thing in the Model "docs.cs" in methods setup and uploadDirect (lines 89 and 133)


To load new files update:
Box.cs:
-Set the BOXSiteURL to the url of your box site (line 22)
-Set the Client Id to your app's id (line 23)
-Set the Client Secret to your app's secret (line 24)
-Generate and update the dveloper_token (line 25)

SharePoint:
-Set the severURL (line 17)
-Set the SharepointURL (line 18)