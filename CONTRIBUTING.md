# How to contribute to Kopano Core

While you can create an account in our [Jira](
https://jira.kopano.io/secure/Signup!default.jspa) to get notified about 
the progress of tickets, we currently do not allow external users to 
create issues or pull request against our repositories. 

If you have found an issue and want to create an issue please either reach 
out to us in our [forum](http://forum.kopano.com), or if you have a 
subscription open up a [support case](https://kopano.com/support/).

To provide a patch please use the following workflow:

- Clone the individual repository from https://stash.kopano.io/
- Apply your changes to your local checkout and please include *"Released
under the Affero GNU General Public License (AGPL) version 3."* in your
commit message, so that we can safely reuse your changes. 
- create a patchfile from your commits. (
https://ariejan.net/2009/10/26/how-to-create-and-apply-a-patch-with-git/ 
does a nice job explaining the details.)
- send the patch to 
[contributing@kopano.io](mailto:contributing@kopano.io) and we'll create 
an issue and a pull request for you.

## Additional notes

- Please only work on one issue per patch.
- If your patch consists of multiple commits stash them into one before 
creating the patch.
- A patch should usually not change more than 50 lines of code (keep your 
changes small).
- Before implementing a new feature get in contact with us so we can 
determine the impact.