<template>
  <data-table
    :value="products"
    table-style="min-width: 50rem"
    sort-mode="">
    <column
      field="code"
      header="Code" />
    <column
      field="name"
      header="Name" />
    <column
      field="category"
      header="Category" />
    <column
      field="quantity"
      header="Quantity" />
  </data-table>
</template>

<script>

export default {
  name: 'PrimeVue',
  data () {
    return {
      products: [
        {
          code: 'code',
          name: 'name',
          category: 'cat',
          quantity: 'quantity'
        }
      ]

    }
  }
}
</script>

<style scoped>

</style>
