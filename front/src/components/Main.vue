<template>
  <div>
    <h1 class="page-header">Movie Management</h1>

    <div class="d-flex justify-content-between">
      <div class="movies-column">
        <table class="table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Title</th>
              <th>Director</th>
              <th>Year</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="movie in movies" :key="movie.id">
              <td>{{ movie.id }}</td>
              <td>{{ movie.title }}</td>
              <td>{{ movie.director }}</td>
              <td>{{ movie.year }}</td>
              <td>
                <div class="action-buttons">
                  <button class="btn btn-warning" @click="editMovie(movie)">Edit</button>
                  <button class="btn btn-danger" @click="deleteMovie(movie.id)">Delete</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="collections-column">
        <div class="box-container mb-3">
          <div class="box">
            <button class="btn btn-primary" @click="openModal">Add Movie</button>
            <button class="btn btn-info" @click="fetchMoviesFromIntegration">Fetch Movies</button>
          </div>
        </div>

        <h5>Collections</h5>
        <ul class="list-group mb-3">
          <li v-for="collection in collections" :key="collection.id" class="list-group-item d-flex justify-content-between align-items-center">
            <div>
              {{ collection.title }}
              <span class="badge bg-secondary ms-2">{{ collection.movieCount }} movie(s)</span>
            </div>
            <button class="btn btn-danger btn-sm" @click="deleteCollection(collection.id)">Delete</button>
          </li>
        </ul>

        <form @submit.prevent="createCollection">
          <div class="mb-3">
            <label for="newCollectionTitle" class="form-label">New Collection Title</label>
            <input v-model="newCollectionTitle" id="newCollectionTitle" type="text" class="form-control" />
          </div>
          <button type="submit" class="btn btn-success">Create Collection</button>
        </form>
      </div>
    </div>

    <Modal
      :movie="selectedMovie"
      :show="showModal"
      :collections="collections"
      @close="closeModal"
      @refresh="handleModalRefresh"
    />
  </div>
</template>

<style scoped>
.page-header {
  text-align: center;
  margin-bottom: 1rem;
}

.box-container {
  display: flex;
  justify-content: center;
  margin-bottom: 1rem;
}

.box {
  padding: 1rem;
  border: 1px solid #ddd;
  border-radius: 0.25rem;
  box-shadow: 0 0 0.125rem rgba(0, 0, 0, 0.075);
  background-color: #f8f9fa;
  min-width: 600px;
}

.box button {
  margin-right: 0.5rem;
}

.table {
  margin-bottom: 0;
}

.action-buttons {
  display: flex;
  gap: 0.5rem;
}

.d-flex {
  display: flex;
}

.justify-content-between {
  justify-content: space-between;
}

.movies-column {
  flex: 3;
  margin-right: 1rem;
  min-width: 600px;
}

.collections-column {
  flex: 3;
  min-width: 600px;
}

.list-group-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.list-group-item .badge {
  font-size: 0.875rem;
  padding: 0.5rem 0.75rem;
}
</style>

<script>
import axios from "axios";
import Modal from "./Modal.vue";

export default {
  components: { Modal },
  data() {
    return {
      movies: [],
      collections: [],
      selectedMovie: null,
      showModal: false,
      newCollectionTitle: "",
    };
  },
  methods: {
    fetchMovies() {
      axios.get("/Movie")
        .then(response => {
          this.movies = response.data;
        });
    },
    fetchCollections() {
      axios.get("/Collection")
        .then(response => {
          const collectionsWithCountPromises = response.data.map(collection => 
            axios.get(`/Collection/${collection.id}/count`)
              .then(countResponse => ({
                ...collection,
                movieCount: countResponse.data
              }))
          );
          Promise.all(collectionsWithCountPromises)
            .then(collections => {
              this.collections = collections;
            });
        });
    },
    fetchMoviesFromIntegration() {
      axios.post("/Integration")
        .then(() => {
          this.fetchMovies();
          this.fetchCollections();
        });
    },
    openModal() {
      this.selectedMovie = null;
      this.showModal = true;
    },
    closeModal() {
      this.showModal = false;
    },
    editMovie(movie) {
      this.selectedMovie = movie;
      this.showModal = true;
    },
    deleteMovie(id) {
      axios.delete(`/Movie/${id}`)
        .then(() => {
          this.fetchMovies();
          this.fetchCollections();
        });
    },
    createCollection() {
      if (!this.newCollectionTitle.trim()) return;
      axios.post("/Collection", { title: this.newCollectionTitle })
        .then(() => {
          this.newCollectionTitle = "";
          this.fetchCollections();
        });
    },
    deleteCollection(id) {
      axios.delete(`/Collection/${id}`)
        .then(() => {
          this.fetchCollections();
        });
    },
    addToCollection(movieId, collectionId) {
      axios.post(`/Collection/${collectionId}/movie/${movieId}`)
        .then(() => {
          this.fetchMovies();
          this.fetchCollections();
        });
    },
    removeFromCollection(movieId, collectionId) {
      axios.delete(`/Collection/${collectionId}/movie/${movieId}`)
        .then(() => {
          this.fetchMovies();
          this.fetchCollections();
        });
    },
    handleModalRefresh() {
      this.fetchCollections();
      this.fetchMovies();
    }
  },
  created() {
    this.fetchMovies();
    this.fetchCollections();
  }
};
</script>