xUnit test class library for the Nyzo.CL nuget package.




Cryptographic operations are tested using the RFC8032 test vectors from section 7.1: https://datatracker.ietf.org/doc/html/rfc8032#section-7.1




The Nyzo.CL nuget package doesn't include this test project due to being unpackable and irrelevant for regular users. Thus requiring the developer to clone the entirety of the construct0/nyzoResources repository hosted on GitHub due to this solution and its project depending on the Nyzo.CL project.




To run the tests, open the Nyzo.CL.sln using Visual Studio or Rider. It may be necessary to manually reference the project due to the test project being dependant on Nyzo.CL and not the other way around.




The version of this project indicates which version of the Nyzo.CL project it belongs to.




Optional template for new test function bodies:
```
// Arrange


// Act


// Assert

```
or
```
throw new NotImplementedException("todo");
// Arrange


// Act


// Assert

```
This strict order can be deviated from if all three occur at some point in the test.