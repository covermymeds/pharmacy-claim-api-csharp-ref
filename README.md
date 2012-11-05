#CoverMyMeds Pharmacy Claim API
##C# Reference Implementation
##Overview
Integrate your pharmacy system with CoverMyMeds to create an "Easy Button" for Prior Authorization. Uses the NCPDP Telecom Claim standard and integration takes only a few days of development time.
*	[Download an Overview Presentation](http://www.covermymeds.com/files/cmm-pharmacy-system-overview.ppt) or [(.pdf)](http://www.covermymeds.com/files/cmm-pharmacy-system-overview.pdf)
*	[Full Technical Documentation](http://www.covermymeds.com/main/pharmacy_claim_api)

Use our [Pharmacy System](http://pharmacysystems.covermymeds.com/) page to see a list of systems that integrate with CoverMyMeds or to contact your system vendor to request integration.
This reference implementation is offered to assist in integrating with the CoverMyMeds Claim API using the Microsoft Visual Studio environment. The master branch is a Visual Studio 2010 solution with a C# console project that prompts a user to submit a claim to the API. An upgraded 2012 version is in the VS2012 branch. Most of the code used to submit data via an HTTP Post to the Claims Service is encapsulated in the Utilities.cs file. Response and error handling as well as preparing the claim data to be submitted is in the Program.cs file. 

##Getting Started
*	Sign up for a free account to learn how CoverMyMeds works. <https://www.covermymeds.com/signup>
*	Watch a short [overview video.](http://help.covermymeds.com/entries/47511-learn-how-to-use-covermymeds-5-minute-silent-video)
*	[Get in touch](mailto:developers@covermymeds.com) and we'll provide an Account Manager to help you through the process and direct access to our senior developers.
*	API Key: [email us to request a key.](mailto:developers@covermymeds.com) We will get back to you the same day.