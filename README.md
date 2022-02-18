# HomeMade.SkyModel
Sky model builder allows the creation of sky models in ASCOM mounts with large number of points (e.g 50).

It connects directly to ASCOM mounts and ASCOM cameras and builds the model via sync (the mount and the driver need to understand it). For example, for EQMOD, setting the behaviour of sync to "add to the list" will build a model. Something similar can be done with Gemini mounts.

![Screenshot](Screenshot.jpg)
