using Moq;
using Wildlife_BLL;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;
using System.Collections.Generic;
using System;

namespace Wildlife_Tests.BLL_Tests;

public class SpeciesServiceTests
{
    private readonly Mock<ISpeciesRepository> _speciesRepoMock;
    private readonly Mock<IExternalSpeciesClient> _externalSpeciesClientMock;
    private readonly SpeciesService _speciesService;

    public SpeciesServiceTests()
    {
        _speciesRepoMock = new Mock<ISpeciesRepository>();
        _externalSpeciesClientMock = new Mock<IExternalSpeciesClient>();
        _speciesService = new SpeciesService(_speciesRepoMock.Object, _externalSpeciesClientMock.Object);
    }

    [Fact]
    public async Task GetSpeciesByIdAsync_ReturnsSpecies_WhenFound()
    {
        // Arrange
        var speciesId = 1;
        var expectedSpecies = new SpeciesDTO { Id = speciesId, ScientificName = "Test Species" };
        _speciesRepoMock.Setup(x => x.GetByIdAsync(speciesId)).ReturnsAsync(expectedSpecies);

        // Act
        var result = await _speciesService.GetSpeciesByIdAsync(speciesId);

        // Assert
        Assert.Equal(expectedSpecies, result);
        _speciesRepoMock.Verify(x => x.GetByIdAsync(speciesId), Times.Once);
    }

    [Fact]
    public async Task GetSpeciesByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var speciesId = 1;
        _speciesRepoMock.Setup(x => x.GetByIdAsync(speciesId)).ReturnsAsync((SpeciesDTO)null);

        // Act
        var result = await _speciesService.GetSpeciesByIdAsync(speciesId);

        // Assert
        Assert.Null(result);
        _speciesRepoMock.Verify(x => x.GetByIdAsync(speciesId), Times.Once);
    }

    [Fact]
    public async Task SearchSpeciesAsync_ReturnsLocalResults()
    {
        // Arrange
        var searchTerm = "bird";
        var limit = 20;
        var expectedSpecies = new List<SpeciesDTO>
        {
            new SpeciesDTO { Id = 1, ScientificName = "Bird Species 1" },
            new SpeciesDTO { Id = 2, ScientificName = "Bird Species 2" }
        };
        _speciesRepoMock.Setup(x => x.SearchAsync(searchTerm, limit)).ReturnsAsync(expectedSpecies);

        // Act
        var result = await _speciesService.SearchSpeciesAsync(searchTerm, limit);

        // Assert
        Assert.Equal(expectedSpecies, result);
        _speciesRepoMock.Verify(x => x.SearchAsync(searchTerm, limit), Times.Once);
    }

    [Fact]
    public async Task FindSpeciesAsync_ReturnsLocalResults_WhenEnoughFound()
    {
        // Arrange
        var searchTerm = "bird";
        var limit = 5;
        var localResults = new List<SpeciesDTO>
        {
            new SpeciesDTO { Id = 1, ScientificName = "Bird Species 1" },
            new SpeciesDTO { Id = 2, ScientificName = "Bird Species 2" },
            new SpeciesDTO { Id = 3, ScientificName = "Bird Species 3" },
            new SpeciesDTO { Id = 4, ScientificName = "Bird Species 4" },
            new SpeciesDTO { Id = 5, ScientificName = "Bird Species 5" }
        };
        _speciesRepoMock.Setup(x => x.SearchAsync(searchTerm, limit)).ReturnsAsync(localResults);

        // Act
        var result = await _speciesService.FindSpeciesAsync(searchTerm, limit);

        // Assert
        Assert.Equal(localResults, result);
        _speciesRepoMock.Verify(x => x.SearchAsync(searchTerm, limit), Times.Once);
        _externalSpeciesClientMock.Verify(x => x.SearchByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task FindSpeciesAsync_ReturnsOnlyLocalResults_WhenSearchTermTooShort()
    {
        // Arrange
        var searchTerm = "cat";
        var limit = 10;
        var localResults = new List<SpeciesDTO>
        {
            new SpeciesDTO { Id = 1, ScientificName = "Cat Species" }
        };
        _speciesRepoMock.Setup(x => x.SearchAsync(searchTerm, limit)).ReturnsAsync(localResults);

        // Act
        var result = await _speciesService.FindSpeciesAsync(searchTerm, limit);

        // Assert
        Assert.Equal(localResults, result);
        _speciesRepoMock.Verify(x => x.SearchAsync(searchTerm, limit), Times.Once);
        _externalSpeciesClientMock.Verify(x => x.SearchByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task FindSpeciesAsync_CombinesLocalAndExternalResults_WhenNeeded()
    {
        // Arrange
        var searchTerm = "eagle";
        var limit = 10;
        var localResults = new List<SpeciesDTO>
        {
            new SpeciesDTO { Id = 1, ScientificName = "Local Eagle" }
        };
        var externalSpecies = new CreateSpeciesDTO
        {
            InaturalistTaxonId = 12345,
            ScientificName = "External Eagle",
            CommonName = "External Eagle",
            ImageUrl = "https://example.com/eagle.jpg",
            IconicTaxonName = "Aves"
        };

        _speciesRepoMock.Setup(x => x.SearchAsync(searchTerm, limit)).ReturnsAsync(localResults);
        _speciesRepoMock.Setup(x => x.GetByTaxonIdAsync(externalSpecies.InaturalistTaxonId)).ReturnsAsync((SpeciesDTO)null);
        _externalSpeciesClientMock.Setup(x => x.SearchByNameAsync(searchTerm)).ReturnsAsync(externalSpecies);

        // Act
        var result = await _speciesService.FindSpeciesAsync(searchTerm, limit);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(localResults[0], result[0]);
        Assert.Equal(0, result[1].Id); // Temporary ID for external species
        Assert.Equal(externalSpecies.ScientificName, result[1].ScientificName);
        _speciesRepoMock.Verify(x => x.SearchAsync(searchTerm, limit), Times.Once);
        _externalSpeciesClientMock.Verify(x => x.SearchByNameAsync(searchTerm), Times.Once);
    }

    [Fact]
    public async Task FindSpeciesAsync_DoesNotAddExternalSpecies_WhenAlreadyExists()
    {
        // Arrange
        var searchTerm = "eagle";
        var limit = 10;
        var localResults = new List<SpeciesDTO>
        {
            new SpeciesDTO { Id = 1, ScientificName = "Local Eagle" }
        };
        var existingSpecies = new SpeciesDTO { Id = 2, InaturalistTaxonId = 12345 };

        _speciesRepoMock.Setup(x => x.SearchAsync(searchTerm, limit)).ReturnsAsync(localResults);
        _speciesRepoMock.Setup(x => x.GetByTaxonIdAsync(12345)).ReturnsAsync(existingSpecies);
        _externalSpeciesClientMock.Setup(x => x.SearchByNameAsync(searchTerm)).ReturnsAsync(new CreateSpeciesDTO { InaturalistTaxonId = 12345 });

        // Act
        var result = await _speciesService.FindSpeciesAsync(searchTerm, limit);

        // Assert
        Assert.Equal(localResults, result);
        _speciesRepoMock.Verify(x => x.GetByTaxonIdAsync(12345), Times.Once);
    }

    [Fact]
    public async Task ImportSpeciesAsync_ReturnsExistingSpecies_WhenAlreadyImported()
    {
        // Arrange
        var taxonId = 12345L;
        var existingSpecies = new SpeciesDTO { Id = 1, InaturalistTaxonId = taxonId };
        _speciesRepoMock.Setup(x => x.GetByTaxonIdAsync(taxonId)).ReturnsAsync(existingSpecies);

        // Act
        var result = await _speciesService.ImportSpeciesAsync(taxonId);

        // Assert
        Assert.Equal(existingSpecies, result);
        _speciesRepoMock.Verify(x => x.GetByTaxonIdAsync(taxonId), Times.Once);
        _externalSpeciesClientMock.Verify(x => x.GetByTaxonIdAsync(taxonId), Times.Never);
    }

    [Fact]
    public async Task ImportSpeciesAsync_ImportsAndReturnsNewSpecies_WhenNotExists()
    {
        // Arrange
        var taxonId = 12345L;
        var externalSpecies = new CreateSpeciesDTO
        {
            InaturalistTaxonId = taxonId,
            ScientificName = "New Species",
            CommonName = "New Species"
        };
        var savedSpecies = new SpeciesDTO { Id = 1, InaturalistTaxonId = taxonId };

        _speciesRepoMock.Setup(x => x.GetByTaxonIdAsync(taxonId)).ReturnsAsync((SpeciesDTO)null);
        _externalSpeciesClientMock.Setup(x => x.GetByTaxonIdAsync(taxonId)).ReturnsAsync(externalSpecies);
        _speciesRepoMock.Setup(x => x.AddAsync(externalSpecies)).ReturnsAsync(savedSpecies);

        // Act
        var result = await _speciesService.ImportSpeciesAsync(taxonId);

        // Assert
        Assert.Equal(savedSpecies, result);
        _speciesRepoMock.Verify(x => x.GetByTaxonIdAsync(taxonId), Times.Once);
        _externalSpeciesClientMock.Verify(x => x.GetByTaxonIdAsync(taxonId), Times.Once);
        _speciesRepoMock.Verify(x => x.AddAsync(externalSpecies), Times.Once);
    }

    [Fact]
    public async Task ImportSpeciesAsync_ReturnsNull_WhenExternalSpeciesNotFound()
    {
        // Arrange
        var taxonId = 12345L;
        _speciesRepoMock.Setup(x => x.GetByTaxonIdAsync(taxonId)).ReturnsAsync((SpeciesDTO)null);
        _externalSpeciesClientMock.Setup(x => x.GetByTaxonIdAsync(taxonId)).ReturnsAsync((CreateSpeciesDTO)null);

        // Act
        var result = await _speciesService.ImportSpeciesAsync(taxonId);

        // Assert
        Assert.Null(result);
        _speciesRepoMock.Verify(x => x.GetByTaxonIdAsync(taxonId), Times.Once);
        _externalSpeciesClientMock.Verify(x => x.GetByTaxonIdAsync(taxonId), Times.Once);
        _speciesRepoMock.Verify(x => x.AddAsync(It.IsAny<CreateSpeciesDTO>()), Times.Never);
    }

    [Fact]
    public async Task ImportSpeciesByTaxonIdAsync_CallsImportSpeciesAsync()
    {
        // Arrange
        var taxonId = 12345L;
        var expectedSpecies = new SpeciesDTO { Id = 1, InaturalistTaxonId = taxonId };
        _speciesRepoMock.Setup(x => x.GetByTaxonIdAsync(taxonId)).ReturnsAsync(expectedSpecies);

        // Act
        var result = await _speciesService.ImportSpeciesByTaxonIdAsync(taxonId);

        // Assert
        Assert.Equal(expectedSpecies, result);
        _speciesRepoMock.Verify(x => x.GetByTaxonIdAsync(taxonId), Times.Once);
    }

    [Fact]
    public async Task GetPopularDutchSpeciesAsync_ReturnsPreloadedSpecies()
    {
        // Arrange
        var limit = 50;
        var expectedSpecies = new List<SpeciesDTO>
        {
            new SpeciesDTO { Id = 1, ScientificName = "Dutch Species 1" },
            new SpeciesDTO { Id = 2, ScientificName = "Dutch Species 2" }
        };
        _speciesRepoMock.Setup(x => x.GetPreloadedSpeciesAsync(limit)).ReturnsAsync(expectedSpecies);

        // Act
        var result = await _speciesService.GetPopularDutchSpeciesAsync(limit);

        // Assert
        Assert.Equal(expectedSpecies, result);
        _speciesRepoMock.Verify(x => x.GetPreloadedSpeciesAsync(limit), Times.Once);
    }

    [Fact]
    public async Task GetSpeciesByClassAsync_ReturnsSpeciesByClass()
    {
        // Arrange
        var className = "Aves";
        var limit = 20;
        var expectedSpecies = new List<SpeciesDTO>
        {
            new SpeciesDTO { Id = 1, ScientificName = "Bird Species" }
        };
        _speciesRepoMock.Setup(x => x.GetByClassificationAsync("class", className, limit)).ReturnsAsync(expectedSpecies);

        // Act
        var result = await _speciesService.GetSpeciesByClassAsync(className, limit);

        // Assert
        Assert.Equal(expectedSpecies, result);
        _speciesRepoMock.Verify(x => x.GetByClassificationAsync("class", className, limit), Times.Once);
    }

    [Fact]
    public async Task GetSpeciesByOrderAsync_ReturnsSpeciesByOrder()
    {
        // Arrange
        var orderName = "Passeriformes";
        var limit = 20;
        var expectedSpecies = new List<SpeciesDTO>
        {
            new SpeciesDTO { Id = 1, ScientificName = "Passerine Species" }
        };
        _speciesRepoMock.Setup(x => x.GetByClassificationAsync("order", orderName, limit)).ReturnsAsync(expectedSpecies);

        // Act
        var result = await _speciesService.GetSpeciesByOrderAsync(orderName, limit);

        // Assert
        Assert.Equal(expectedSpecies, result);
        _speciesRepoMock.Verify(x => x.GetByClassificationAsync("order", orderName, limit), Times.Once);
    }

    [Fact]
    public async Task GetSpeciesByFamilyAsync_ReturnsSpeciesByFamily()
    {
        // Arrange
        var familyName = "Corvidae";
        var limit = 20;
        var expectedSpecies = new List<SpeciesDTO>
        {
            new SpeciesDTO { Id = 1, ScientificName = "Crow Species" }
        };
        _speciesRepoMock.Setup(x => x.GetByClassificationAsync("family", familyName, limit)).ReturnsAsync(expectedSpecies);

        // Act
        var result = await _speciesService.GetSpeciesByFamilyAsync(familyName, limit);

        // Assert
        Assert.Equal(expectedSpecies, result);
        _speciesRepoMock.Verify(x => x.GetByClassificationAsync("family", familyName, limit), Times.Once);
    }
}