<template>
  <div v-if="show" class="modal fade show" tabindex="-1" style="display: block;" aria-modal="true" role="dialog">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">{{ isEditing ? 'Edit Movie' : 'Add Movie' }}</h5>
          <button type="button" class="btn-close" @click="close"></button>
        </div>
        <form @submit.prevent="submitForm">
          <div class="modal-body">
            <div class="mb-3">
              <label for="title" class="form-label">Title</label>
              <input v-model="title" id="title" type="text" class="form-control" />
              <div v-if="v$.title.$error" class="text-danger">Title is required.</div>
            </div>
            <div class="mb-3">
              <label for="director" class="form-label">Director</label>
              <input v-model="director" id="director" type="text" class="form-control" />
            </div>
            <div class="mb-3">
              <label for="year" class="form-label">Year</label>
              <input v-model="year" id="year" type="number" class="form-control" />
              <div v-if="v$.year.$error" class="text-danger">
                <div v-if="!v$.year.required.$response">Year is required.</div>
                <div v-if="!v$.year.between.$response">Year must be between 1900 and 2200.</div>
              </div>
            </div>
            <div class="mb-3">
              <label for="rate" class="form-label">Rate</label>
              <input v-model="rate" id="rate" type="number" step="0.1" class="form-control" />
            </div>
            <div class="mb-3">
              <label for="collection" class="form-label">Collection</label>
              <select v-model="selectedCollectionId" id="collection" class="form-select">
                <option value="">None</option>
                <option v-for="collection in collections" :key="collection.id" :value="collection.id">
                  {{ collection.title }}
                </option>
              </select>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="close">Close</button>
            <button type="submit" class="btn btn-primary">{{ isEditing ? 'Save Changes' : 'Add Movie' }}</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import { reactive, toRefs, ref, watch } from 'vue';
import useVuelidate from '@vuelidate/core';
import { required, between } from '@vuelidate/validators';
import axios from 'axios';

export default {
  props: {
    movie: {
      type: Object,
      default: () => null,
    },
    show: Boolean,
    collections: Array,
  },
  setup(props, { emit }) {
    const movie = reactive({
      id: props.movie?.id || null,
      title: props.movie?.title || '',
      director: props.movie?.director || '',
      year: props.movie?.year || null,
      rate: props.movie?.rate || null,
    });

    const selectedCollectionId = ref(props.movie?.collectionId || null);

    const isEditing = ref(!!props.movie?.id);

    watch(
      () => props.movie,
      (newMovie) => {
        movie.id = newMovie?.id || null;
        movie.title = newMovie?.title || '';
        movie.director = newMovie?.director || '';
        movie.year = newMovie?.year || null;
        movie.rate = newMovie?.rate || null;
        selectedCollectionId.value = newMovie?.collectionId || null;
        isEditing.value = !!newMovie?.id;
      },
      { immediate: true }
    );

    const rules = {
      title: { required },
      year: { required, between: between(1900, 2200) },
    };

    const v$ = useVuelidate(rules, movie);

    const close = () => emit('close');

    const submitForm = () => {
      v$.value.$touch();
      if (v$.value.$invalid) {
        return;
      }
      const method = movie.id ? 'put' : 'post';
      const url = movie.id ? `/Movie/${movie.id}` : '/Movie';
      const collectionId = selectedCollectionId.value === '' ? null : selectedCollectionId.value;

      axios[method](url, { ...movie, collectionId })
        .then(() => {
          if (movie.id && collectionId) {
            axios.post(`/Collection/${collectionId}/movie/${movie.id}`);
          } else if (movie.id && !collectionId && movie.collectionId) {
            axios.delete(`/Collection/${movie.collectionId}/movie/${movie.id}`);
          }
          emit('refresh');
          close();
        });
    };

    return {
      ...toRefs(movie),
      selectedCollectionId,
      v$,
      isEditing,
      close,
      submitForm,
    };
  },
};
</script>
